using Puzzle.Application.Boosters;
using Puzzle.Application.Levels;
using Puzzle.Application.Progress;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Rules;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Config;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Input;
using Puzzle.Presentation.Persistence;
using Puzzle.Presentation.Screens;
using UnityEngine;

namespace Puzzle.Bootstrap
{
    /// <summary>
    /// Composition root của scene Game: tạo mọi đối tượng C#, trao cho presenter, chọn level để chơi
    /// và lưu tiến độ. Muốn biết ai phụ thuộc ai thì đọc file này.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LevelCatalog _catalog;
        [SerializeField] private BoardLayoutConfig _layoutConfig;
        [SerializeField] private PaletteConfig _palette;
        [SerializeField] private InputConfig _inputConfig;
        [SerializeField] private FeedbackConfig _feedbackConfig;
        [SerializeField] private ClueVisuals _clueVisuals;
        [SerializeField, Tooltip("Khoá PlayerPrefs lưu tiến độ.")] private string _saveKey = "puzzle.progress.v1";

        [Header("Views")]
        [SerializeField] private Camera _camera;
        [SerializeField] private BoardView _board;
        [SerializeField] private BoardInput _boardInput;
        [SerializeField] private HudView _hud;
        [SerializeField] private SolvedPopupView _popup;
        [SerializeField] private CelebrationView _celebration;
        [SerializeField] private RuleLegendView _legend;
        [SerializeField] private PaletteView _paletteView;

        private IProgressStore _store;
        private PlayerProgress _progress;
        private LevelLoader _loader;
        private GameViews _views;
        private GameServices _services;
        private LevelScope _scope;
        private PalettePresenter _palettePresenter;
        private int _levelIndex;

        private void Start()
        {
            _store = new PlayerPrefsProgressStore(_saveKey);
            _progress = _store.Load();
            _loader = new LevelLoader(RuleRegistry.CreateDefault());
            _views = new GameViews
            {
                Board = _board, BoardInput = _boardInput, Hud = _hud, Popup = _popup, Celebration = _celebration, Legend = _legend,
                Palette = _paletteView,
            };
            _services = new GameServices
            {
                LayoutConfig = _layoutConfig, Palette = _palette, FeedbackConfig = _feedbackConfig, ClueVisuals = _clueVisuals,
                Hint = new HintBooster(), Check = new CheckBooster(), Haptics = new DeviceHaptics(_feedbackConfig),
                Paint = new PaintSelection(_palette.Paints.Count),
            };

            if (_camera != null) _camera.backgroundColor = _palette.Background;
            _boardInput.Construct(_board, _inputConfig);
            _palettePresenter = new PalettePresenter(_paletteView, _services.Paint, _palette);
            StartLevel(new LevelSelector(_catalog, _progress).FirstToPlay());
        }

        private void StartLevel(int index)
        {
            _scope?.Dispose();
            _levelIndex = index;
            LevelAsset asset = _catalog.Levels[index];
            LevelDocument level = _loader.Parse(asset.Source, asset.Id);

            PuzzleState saved = null;
            if (_progress.HasPuzzleInProgress && _progress.InProgress.LevelId == level.Id)
            {
                _progress.InProgress.TryRestore(level.Puzzle, out saved);
            }

            _scope = new LevelScope(level, saved, $"Màn {index + 1}", index == _catalog.Count - 1, _views, _services);
            _scope.Solved += OnSolved;
            _scope.NextRequested += OnNextRequested;
        }

        private void OnSolved()
        {
            _progress.MarkCompleted(_scope.LevelId);
            _store.Save(_progress);
        }

        private void OnNextRequested() => StartLevel((_levelIndex + 1) % _catalog.Count);

        // Mobile: app vào nền có thể bị kill bất cứ lúc nào, nên lưu ván dở ngay lúc này (ARCHITECTURE.md mục 14).
        private void OnApplicationPause(bool paused)
        {
            if (paused) SaveInProgress();
        }

        private void OnApplicationQuit() => SaveInProgress();

        private void SaveInProgress()
        {
            if (_scope == null || _store == null || _scope.Session.Snapshot.IsSolved) return;
            _progress.InProgress = SavedPuzzle.Capture(_scope.LevelId, _scope.Session.Snapshot.State);
            _store.Save(_progress);
        }

        private void OnDestroy()
        {
            _scope?.Dispose();
            _palettePresenter?.Dispose();
        }
    }
}
