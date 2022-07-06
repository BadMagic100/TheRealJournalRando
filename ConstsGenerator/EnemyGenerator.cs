using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ConstsGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class EnemyGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            IEnumerable<Dictionary<string, object>> data = context.AdditionalFiles.Where(f => f.Path.EndsWith("JournalData.json"))
                .Select(t => t.GetText(context.CancellationToken)?.ToString())
                .Where(t => t != null)
                .SelectMany(x => JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(x!));
            

            if (!data.Any())
            {
                return;
            }
            const string delimiter = "\n        ";

            string src = $@"//Auto-generated
namespace TheRealJournalRando.Data.Generated
{{
    public static class EnemyNames
    {{
        {string.Join(delimiter, data.Select(t => $"public const string {NameUtils.SafeName(t["icName"].ToString())} = \"{t["icName"]}\";"))}
    }}
}}
";
            context.AddSource("EnemyNames.g.cs", src);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            GenTimeDependencies.AddOnce();
        }
    }
}
