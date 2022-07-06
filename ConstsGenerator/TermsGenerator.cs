using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ConstsGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class TermsGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AdditionalFiles.FirstOrDefault(f => f.Path.EndsWith("terms.json")) is not AdditionalText termsJson)
            {
                return;
            }
            if (termsJson.GetText(context.CancellationToken)?.ToString() is not string content)
            {
                return;
            }

            List<string>? terms = JsonConvert.DeserializeObject<List<string>>(content);

            if (terms == null || terms.Count == 0)
            {
                return;
            }
            const string delimiter = "\n        ";

            string src = $@"//Auto-generated
namespace TheRealJournalRando.Rando.Generated
{{
    public static class Terms
    {{
        {string.Join(delimiter, terms.Select(t => $"public const string {t} = \"{t}\";"))}
    }}
}}
";
            context.AddSource("Terms.g.cs", src);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            GenTimeDependencies.AddOnce();
        }
    }
}
