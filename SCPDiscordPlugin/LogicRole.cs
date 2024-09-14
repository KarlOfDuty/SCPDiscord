using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SCPDiscord
{
	public enum LogicType
	{
		None,
		And,
		Or,
		NotAnd,
		PrimaryOnly,
		Xor,
		NotOr
	}

	[JsonObject]
	public class LogicRole
	{
		public LogicType Type { get; set; }
		public List<ulong> Roles { get; set; }
		[CanBeNull] public List<string> Commands { get; set; }
		[CanBeNull] public Dictionary<int, LogicRole> Children { get; set; }

		public bool IsPermitted(List<ulong> userRoles)
		{
			return Type switch
			{
				LogicType.None => true,
				LogicType.And => Roles.All(userRoles.Contains),
				LogicType.Or => Roles.Any(userRoles.Contains),
				LogicType.NotAnd => !Roles.All(userRoles.Contains),
				LogicType.PrimaryOnly => userRoles.Intersect(Roles).Any() && userRoles.Count == 1,
				LogicType.Xor =>
					// If the user has any of the roles, but not all of them
					Roles.Any(userRoles.Contains) && Roles.Count != userRoles.Count,
				LogicType.NotOr => !Roles.Any(userRoles.Contains),
				_ => false
			};
		}
	}

	public class RoleProcessor
	{
		private readonly Dictionary<int, LogicRole> _logicRoles;

		public HashSet<ulong> ProcessedRoleIds { get; }

		public RoleProcessor(JObject json)
		{
			_logicRoles = JsonConvert.DeserializeObject<Dictionary<int, LogicRole>>(json.SelectToken("logicroles").ToString());
			ProcessedRoleIds = new HashSet<ulong>();
			FillProcessedRoleIds();
		}

		private void FillProcessedRoleIds()
		{
			foreach (var role in _logicRoles.Values)
			{
				AddRoleIds(role);
			}
		}

		private void AddRoleIds(LogicRole role)
		{
			foreach (var roleId in role.Roles)
			{
				ProcessedRoleIds.Add(roleId);
			}

			if (role.Children == null) return;

			foreach (var child in role.Children.Values)
			{
				AddRoleIds(child);
			}
		}

		public List<string> ProcessRoles(List<ulong> userRoles)
		{
			var commands = new List<string>();
			foreach (var key in _logicRoles.Keys.OrderBy(k => k))
			{
				var role = _logicRoles[key];
				var roleCommands = ProcessRole(role, userRoles);
				if (!roleCommands.Any()) continue;
				commands.AddRange(roleCommands);
				break;
			}

			return commands;
		}

		private List<string> ProcessRole(LogicRole role, List<ulong> userRoles)
		{
			var commands = new List<string>();
			if (role.Type == LogicType.None) return commands;

			var hasRole = role.IsPermitted(userRoles);
			if (!hasRole) return commands;

			if (role.Commands != null) commands.AddRange(role.Commands);
			if (role.Children == null) return commands;
			foreach (var child in role.Children.OrderBy(x => x.Key))
			{
				var childCommands = ProcessRole(child.Value, userRoles);
				if (childCommands.Any())
				{
					commands.AddRange(childCommands);
					break;
				}
			}

			return commands;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var key in _logicRoles.Keys.OrderBy(k => k))
			{
				var role = _logicRoles[key];
				BuildRoleString(role, sb, 0);
			}
			return sb.ToString();
		}

		private static void BuildRoleString(LogicRole role, StringBuilder sb, int indentLevel)
		{
			var indent = new string(' ', indentLevel * 2);
			sb.AppendLine($"{indent}Role: {string.Join(role.Type.ToString(), role.Roles)}");
			sb.AppendLine($"{indent}Commands: {string.Join(", ", role.Commands ?? new List<string>())}");
			if (role.Children == null) return;
			foreach (var child in role.Children)
			{
				BuildRoleString(child.Value, sb, indentLevel + 1);
			}
		}
	}

}
