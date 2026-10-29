using System;
using System.Collections.Generic;
using Puzzle.Domain.Rules.ShapeBank;

namespace Puzzle.Domain.Rules
{
    /// <summary>
    /// Chỗ duy nhất biết game có những luật nào: map id (dùng trong dữ liệu level) → hàm tạo luật.
    /// Thêm luật mới = thêm một dòng trong <see cref="CreateDefault"/>, không sửa nơi nào khác.
    /// Là đối tượng thường (không static) để test có thể tạo registry riêng.
    /// </summary>
    public sealed class RuleRegistry
    {
        private readonly Dictionary<string, Func<RuleParameters, IRule>> _factories =
            new Dictionary<string, Func<RuleParameters, IRule>>(StringComparer.Ordinal);

        public IEnumerable<string> RuleIds => _factories.Keys;

        /// <summary>Registry chứa mọi luật của game.</summary>
        public static RuleRegistry CreateDefault()
        {
            var registry = new RuleRegistry();
            registry.Register(ShapeBankRule.Id, p => new ShapeBankRule(p.Shapes));
            return registry;
        }

        public void Register(string ruleId, Func<RuleParameters, IRule> factory)
        {
            if (string.IsNullOrEmpty(ruleId)) throw new ArgumentException("Rule id is required.", nameof(ruleId));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (_factories.ContainsKey(ruleId)) throw new ArgumentException($"Rule '{ruleId}' is already registered.");
            _factories[ruleId] = factory;
        }

        public bool Contains(string ruleId) => ruleId != null && _factories.ContainsKey(ruleId);

        /// <summary>Tạo luật theo id; ném lỗi nếu id lạ để level hỏng bị phát hiện ngay khi nạp.</summary>
        public IRule Create(string ruleId, RuleParameters parameters)
        {
            if (!Contains(ruleId)) throw new KeyNotFoundException($"Unknown rule '{ruleId}'.");
            return _factories[ruleId](parameters ?? RuleParameters.Empty);
        }
    }
}
