using YamlDotNet.RepresentationModel;

namespace versioning_manager_api.Middle.DockerCompose;

/// <summary>
/// The docker-compose file helper service.
/// </summary>
public class DockerComposeHelper
{
    /// <summary>
    /// Gets the total docker-compose file 
    /// </summary>
    /// <param name="composes">The docker-compose files.</param>
    /// <returns>The stream with merged composes.</returns>
    public Stream GetTotalCompose(IEnumerable<string> composes)
    {
        YamlMappingNode mergedRoot = new();

        foreach (string file in composes)
        {
            if (string.IsNullOrWhiteSpace(file))
                continue;
            YamlStream yaml = [];
            yaml.Load(new StringReader(file));
            YamlMappingNode rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;

            MergeNodes(mergedRoot, rootNode);
        }
        
        MemoryStream result = new();
        using StreamWriter writer = new(result, leaveOpen: true);
        YamlDocument mergedDoc = new(mergedRoot);
        new YamlStream(mergedDoc).Save(writer, assignAnchors: false);
        return result;
    }
    
    private static void MergeNodes(YamlMappingNode target, YamlMappingNode source)
    {
        foreach (KeyValuePair<YamlNode, YamlNode> sourceEntry in source.Children)
        {
            YamlScalarNode key = (YamlScalarNode)sourceEntry.Key;
            YamlNode sourceValue = sourceEntry.Value;

            if (target.Children.TryGetValue(key, out YamlNode? existingValue))
            {
                switch (existingValue)
                {
                    case YamlMappingNode existingMapping when sourceValue is YamlMappingNode sourceMapping:
                        MergeMappings(existingMapping, sourceMapping);
                        break;
                    case YamlSequenceNode existingSequence when sourceValue is YamlSequenceNode sourceSequence:
                        MergeSequences(existingSequence, sourceSequence);
                        break;
                    default:
                        target.Children[key] = sourceValue;
                        break;
                }
            }
            else
            {
                target.Children.Add(key, sourceValue);
            }
        }
    }
    
    private static void MergeMappings(YamlMappingNode target, YamlMappingNode source)
    {
        foreach (KeyValuePair<YamlNode, YamlNode> sourceEntry in source.Children)
        {
            YamlScalarNode key = (YamlScalarNode)sourceEntry.Key;
            YamlNode sourceValue = sourceEntry.Value;

            if (target.Children.TryGetValue(key, out YamlNode? existingValue))
            {
                switch (existingValue)
                {
                    case YamlMappingNode existingMapping when sourceValue is YamlMappingNode sourceMapping:
                        MergeMappings(existingMapping, sourceMapping);
                        break;
                    case YamlSequenceNode existingSequence when sourceValue is YamlSequenceNode sourceSequence:
                        MergeSequences(existingSequence, sourceSequence);
                        break;
                    default:
                        target.Children[key] = sourceValue;
                        break;
                }
            }
            else
            {
                target.Children.Add(key, sourceValue);
            }
        }
    }
    
    private static void MergeSequences(YamlSequenceNode target, YamlSequenceNode source)
    {
        foreach (YamlNode item in source.Children)
        {
            target.Children.Add(item);
        }
    }
    
}
/// <summary>
/// The docker-compose helper service collection extensions.
/// </summary>
public static class DockerComposeHelperServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="DockerComposeHelper"/> to <paramref name="services"/> as <c>singleton</c>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>An instance of <paramref name="services"/>.</returns>
    public static IServiceCollection AddDockerComposeHelper(this IServiceCollection services)
    {
        return services.AddSingleton<DockerComposeHelper>();
    }
}