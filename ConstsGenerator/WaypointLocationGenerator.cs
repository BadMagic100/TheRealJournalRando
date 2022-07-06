using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ConstsGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class WaypointGenerator : WaypointLocationGenerator
    {
        public WaypointGenerator() : base("waypoints.json", "Waypoints") { }
    }

    [Generator(LanguageNames.CSharp)]
    public class LocationGenerator : WaypointLocationGenerator
    {
        public LocationGenerator() : base("enemyLocations.json", "Locations") { }
    }

    public class WaypointLocationGenerator : ISourceGenerator
    {
        private readonly string inputFileName;
        private readonly string outputClassName;

        public WaypointLocationGenerator(string inputFileName, string outputClassName)
        {
            this.inputFileName = inputFileName;
            this.outputClassName = outputClassName;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AdditionalFiles.FirstOrDefault(f => f.Path.EndsWith(inputFileName)) is not AdditionalText logicJson)
            {
                return;
            }
            if (logicJson.GetText(context.CancellationToken)?.ToString() is not string content)
            {
                return;
            }

            List<Dictionary<string, string>>? logicDefs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(content);

            if (logicDefs == null || logicDefs.Count == 0)
            {
                return;
            }
            const string delimiter = "\n        ";

            string src = $@"//Auto-generated
namespace TheRealJournalRando.Rando.Generated
{{
    public static class {outputClassName}
    {{
        {string.Join(delimiter, logicDefs.Select(t => $"public const string {NameUtils.SafeName(t["name"])} = \"{t["name"]}\";"))}
    }}
}}
";
            context.AddSource($"{outputClassName}.g.cs", src);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            GenTimeDependencies.AddOnce();
        }
    }
}
