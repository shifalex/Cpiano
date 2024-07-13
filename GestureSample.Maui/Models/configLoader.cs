using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
namespace GestureSample.Maui.Models
{
    

    public class ConfigLoader
    {
        public static async Task<List<GameConfig>> LoadAllConfigsAsync()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "GestureSample.Maui.Resources.jsconfig1.json";

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            using StreamReader reader = new StreamReader(stream);
            var jsonString = await reader.ReadToEndAsync();

            var configurationsWrapper = JsonSerializer.Deserialize<ConfigurationsWrapper>(jsonString);
            return configurationsWrapper.Configurations;
        }
    }
}
