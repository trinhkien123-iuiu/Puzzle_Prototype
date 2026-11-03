"""Convert The Artisan of Glimmith .puz levels into this project's .pzl format (Docs/design/level-format.md).

.pzl currently supports only `rule shape-bank` and no given borders, so a level is converted only when
every clue it uses maps onto that. Anything else is skipped with a reason; --lossy writes it anyway with
the dropped clues listed in comments (the puzzle may then have more than one solution).

Output defaults to a folder outside Assets/: every .pzl under Assets/ becomes a catalog level, and the
level-format rules forbid shipping another game's levels.

usage: python tools/puz2pzl.py <puz root> [out dir] [--lossy]
"""
import sys
from pathlib import Path

import puz

DEFAULT_OUT = Path(r"D:\GitHub\Glimmith_Reference\pzl")
LABELS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
SUPPORTED_HEADER = {"VERSION", "PUZZLE_VERSION", "DIFFICULTY", "DIMENSIONS", "SHAPE_BANK"}


def unsupported(p):
    """Clues in the .puz that .pzl cannot express."""
    out = [k for k in p["header"] if k not in SUPPORTED_HEADER]
    out += sorted({t for t in p["cells"].values() if t != ".."})
    edges = {"given border" if t in ("#", "##") else t for t in [*p["vedges"].values(), *p["hedges"].values()]}
    return out + sorted(edges)


def label_regions(p):
    """One label per region; reuse labels between non-adjacent regions only if there are more than 36."""
    regions = p["regions"]
    if len(regions) <= len(LABELS):
        return {rc: LABELS[i] for i, cells in enumerate(regions) for rc in cells}
    reg = {rc: i for i, cells in enumerate(regions) for rc in cells}
    neighbours = [set() for _ in regions]
    for (r, c), i in reg.items():
        for n in ((r + 1, c), (r, c + 1)):
            if n in reg and reg[n] != i:
                neighbours[i].add(reg[n])
                neighbours[reg[n]].add(i)
    colour = {}
    for i in range(len(regions)):
        used = {colour[j] for j in neighbours[i] if j in colour}
        colour[i] = next(ch for ch in LABELS if ch not in used)
    return {rc: colour[reg[rc]] for rc in reg}


def solution_rows(p):
    label = label_regions(p)
    rs = [r for r, _ in label]
    cs = [c for _, c in label]
    return [
        "".join(label.get((r, c), ".") for c in range(min(cs), max(cs) + 1))
        for r in range(min(rs), max(rs) + 1)
    ]


def shape_rows(rows):
    width = max(len(r) for r in rows)
    return ["".join("#" if ch == "#" else "." for ch in r.ljust(width)) for r in rows]


def convert(path, lossy=False):
    """Return (pzl text, None) or (None, reason it was skipped)."""
    p = puz.parse(path)
    if not p["regions"]:
        return None, "no SOLUTION"
    if "SHAPE_BANK" not in p["header"]:
        return None, "no shape-bank rule (.pzl needs at least one supported rule)"
    dropped = unsupported(p)
    if dropped and not lossy:
        return None, "unsupported: " + ", ".join(dropped)
    errs = puz.check(p)
    if errs:
        return None, "solution fails its own rules: " + "; ".join(errs)

    shapes = {s["id"]: s for s in p["shapes"]}
    bank = [i for i in p["header"]["SHAPE_BANK"] if i in shapes]
    lines = [f"// Converted from The Artisan of Glimmith: {Path(path).name}. Reference only, do not ship."]
    if dropped:
        lines.append("// LOSSY: dropped clues " + ", ".join(dropped) + " (solution may not be unique).")
    lines += [f"id: glim-{Path(path).stem.lower()}", f"difficulty: {min(max(p['header']['DIFFICULTY'][0], 1), 5)}", ""]
    for i in bank:
        lines.append(f"shape S{i}")
        lines += shape_rows(shapes[i]["rows"])
        lines.append("")
    lines += ["rule shape-bank " + " ".join(f"S{i}" for i in bank), "", "solution"]
    lines += solution_rows(p)
    return "\n".join(lines) + "\n", None


def main(argv):
    lossy = "--lossy" in argv
    args = [a for a in argv if a != "--lossy"]
    src = Path(args[0])
    out = Path(args[1]) if len(args) > 1 else DEFAULT_OUT
    written, skipped = 0, []
    for f in sorted(src.rglob("*.puz")):
        text, reason = convert(f, lossy)
        if text is None:
            skipped.append((f.relative_to(src), reason))
            continue
        dest = out / f.relative_to(src).with_suffix(".pzl")
        dest.parent.mkdir(parents=True, exist_ok=True)
        dest.write_text(text, encoding="utf-8")
        written += 1
    print(f"wrote {written} level(s) to {out}")
    for name, reason in skipped:
        print(f"  skip {name}: {reason}")


if __name__ == "__main__":
    main(sys.argv[1:])
