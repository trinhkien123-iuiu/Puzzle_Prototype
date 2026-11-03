"""Parser for The Artisan of Glimmith .puz level files (plain-text, VERSION 1).

Grid geometry (W cols x H rows, from DIMENSIONS W H):
  - 2H+1 text lines, each up to 3W+1 chars.
  - Cell (r, c) content: line 2r+1, chars [3c+1 : 3c+3]  ("..", "P1", "S3", ...; "  " = not on board)
  - Vertical edge left of cell (r, c): line 2r+1, char 3c
  - Horizontal edge above cell (r, c): line 2r, chars [3c+1 : 3c+3]
  - Corners ('+') sit at line 2r, char 3c.
In SOLUTION, an edge of '#' is a region boundary and ' ' means the two cells share a region.
"""
import json
import sys
from pathlib import Path


def _split_sections(lines):
    header, sections, shapes = {}, {}, []
    i = 0
    while i < len(lines):
        line = lines[i]
        key, _, rest = line.partition(" ")
        if key in ("PUZZLE", "SOLUTION"):
            sections[key] = i + 1
            i += 1
            continue
        if key == "SHAPE":
            # SHAPE <id> <rows>, followed by <rows> lines of '#' pixels
            sid, rows = (int(x) for x in rest.split())
            shapes.append({"id": sid, "rows": [l.rstrip("\n") for l in lines[i + 1:i + 1 + rows]]})
            i += 1 + rows
            continue
        if key.isupper() and key.replace("_", "").isalpha() and "PUZZLE" not in sections:
            vals = rest.split()
            header[key] = [int(v) if v.lstrip("-").isdigit() else v for v in vals] if vals else True
        i += 1
    return header, sections, shapes


def _grid(lines, start, w, h):
    rows = [lines[start + k].rstrip("\n") if start + k < len(lines) else "" for k in range(2 * h + 1)]
    return [r.ljust(3 * w + 1) for r in rows]


def parse(path):
    lines = Path(path).read_text(encoding="utf-8").splitlines()
    header, sections, shapes = _split_sections(lines)
    w, h = header["DIMENSIONS"]
    pg = _grid(lines, sections["PUZZLE"], w, h)

    cells, vedges, hedges = {}, {}, {}
    for r in range(h):
        for c in range(w):
            tok = pg[2 * r + 1][3 * c + 1:3 * c + 3]
            if tok != "  ":
                cells[(r, c)] = tok
    for r in range(h):
        for c in range(w + 1):
            ch = pg[2 * r + 1][3 * c]
            if ch not in " |":
                vedges[(r, c)] = ch  # edge between (r, c-1) and (r, c)
    for r in range(h + 1):
        for c in range(w):
            tok = pg[2 * r][3 * c + 1:3 * c + 3]
            if tok not in ("  ", "--"):
                hedges[(r, c)] = tok  # edge between (r-1, c) and (r, c)

    regions = None
    if "SOLUTION" in sections:
        sg = _grid(lines, sections["SOLUTION"], w, h)
        region_of, regions = {}, []
        for start in cells:
            if start in region_of:
                continue
            rid, stack, members = len(regions), [start], []
            region_of[start] = rid
            while stack:
                r, c = stack.pop()
                members.append((r, c))
                nbrs = [
                    ((r, c + 1), sg[2 * r + 1][3 * c + 3]),
                    ((r, c - 1), sg[2 * r + 1][3 * c]),
                    ((r + 1, c), sg[2 * r + 2][3 * c + 1]),
                    ((r - 1, c), sg[2 * r][3 * c + 1]),
                ]
                for n, edge in nbrs:
                    if edge == " " and n in cells and n not in region_of:
                        region_of[n] = rid
                        stack.append(n)
            regions.append(sorted(members))

    return {
        "file": str(path),
        "header": header,
        "shapes": shapes,
        "width": w,
        "height": h,
        "cells": cells,
        "vedges": vedges,
        "hedges": hedges,
        "regions": regions,
    }


def canon(cells, mirror=True):
    """Canonical form of a polyomino under rotation (and reflection if mirror)."""
    best, cur = None, [tuple(x) for x in cells]
    for m in ([False, True] if mirror else [False]):
        b = [(r, -c) for r, c in cur] if m else cur
        for _ in range(4):
            r0, c0 = min(r for r, _ in b), min(c for _, c in b)
            key = tuple(sorted((r - r0, c - c0) for r, c in b))
            best = key if best is None or key < best else best
            b = [(c, -r) for r, c in b]
    return best


def shape_cells(rows):
    return [(r, c) for r, row in enumerate(rows) for c, ch in enumerate(row) if ch == "#"]


# Edge clue -> which side must hold the smaller region ("a" = left/top cell, "b" = right/bottom cell).
ARROW_SMALLER = {"<": "a", ">": "b", "^^": "a", "vv": "b"}


def check(p, regions=None):
    """Return a list of rule violations for a region partition (defaults to the file's SOLUTION).

    Rules are inferred from the shipped solutions, not from game code; see docs/puz-format.md.
    """
    regions = regions if regions is not None else p["regions"]
    reg = {rc: i for i, cells in enumerate(regions) for rc in cells}
    shape = [canon(cells) for cells in regions]
    errs = []

    edges = [((r, c - 1), (r, c), t) for (r, c), t in p["vedges"].items()]
    edges += [((r - 1, c), (r, c), t) for (r, c), t in p["hedges"].items()]
    for a, b, t in edges:
        if a not in reg or b not in reg:
            continue
        ra, rb = reg[a], reg[b]
        if ra == rb:
            errs.append(f"edge {t} {a}-{b}: both cells in one region")
        elif t in ("=", "==") and shape[ra] != shape[rb]:
            errs.append(f"edge {t} {a}-{b}: regions must be congruent")
        elif t in ("!", "!!") and shape[ra] == shape[rb]:
            errs.append(f"edge {t} {a}-{b}: regions must differ in shape")
        elif t in ARROW_SMALLER:
            small, big = (ra, rb) if ARROW_SMALLER[t] == "a" else (rb, ra)
            if not len(regions[small]) < len(regions[big]):
                errs.append(f"edge {t} {a}-{b}: arrow must point at the smaller region")

    kinds = {}
    for rc, t in p["cells"].items():
        kinds.setdefault(t[0], {}).setdefault(t, []).append(reg[rc])
    for t, rs in kinds.get("P", {}).items():
        if sorted(rs) != list(range(len(regions))):
            errs.append(f"{t}: every region must contain exactly one {t}")
    s_shape = {}
    for t, rs in kinds.get("S", {}).items():
        shapes = {shape[x] for x in rs}
        if len(shapes) > 1:
            errs.append(f"{t}: regions marked {t} must be congruent")
        s_shape[t] = shapes
    s_list = list(s_shape.items())
    for i in range(len(s_list)):
        for j in range(i + 1, len(s_list)):
            if s_list[i][1] & s_list[j][1]:
                errs.append(f"{s_list[i][0]}/{s_list[j][0]}: different S numbers must differ in shape")

    h = p["header"]
    if "SHAPE_BANK" in h:
        bank = {s["id"]: canon(shape_cells(s["rows"])) for s in p["shapes"]}
        allowed = {bank[i] for i in h["SHAPE_BANK"] if i in bank}
        errs += [f"region {i}: not a SHAPE_BANK shape" for i, s in enumerate(shape) if s not in allowed]
    if "AREA_EQUALS" in h:
        errs += [f"region {i}: area != {h['AREA_EQUALS'][0]}" for i, c in enumerate(regions) if len(c) != h["AREA_EQUALS"][0]]
    if "NO_RECTANGLES" in h:
        for i, cells in enumerate(regions):
            rs, cs = [r for r, _ in cells], [c for _, c in cells]
            if len(cells) == (max(rs) - min(rs) + 1) * (max(cs) - min(cs) + 1):
                errs.append(f"region {i}: rectangles not allowed")
    return errs


def to_json(p):
    """JSON-friendly form: cells as [r, c, token], regions as lists of [r, c]."""
    return {
        "header": p["header"],
        "shapes": p["shapes"],
        "width": p["width"],
        "height": p["height"],
        "cells": [[r, c, t] for (r, c), t in sorted(p["cells"].items())],
        "vedges": [[r, c, t] for (r, c), t in sorted(p["vedges"].items())],
        "hedges": [[r, c, t] for (r, c), t in sorted(p["hedges"].items())],
        "regions": [[list(rc) for rc in reg] for reg in p["regions"]] if p["regions"] else None,
    }


if __name__ == "__main__":
    # usage: python puz.py <puz root dir> <out dir>
    src, out = Path(sys.argv[1]), Path(sys.argv[2])
    for f in sorted(src.rglob("*.puz")):
        dest = out / f.relative_to(src).with_suffix(".json")
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(json.dumps(to_json(parse(f)), separators=(",", ":")), encoding="utf-8")
        print(dest)
