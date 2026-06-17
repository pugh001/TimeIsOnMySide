using Capitec.StrategicProjects.Shared.Hosting;
using RestService.Common.Exceptions;

namespace RestService.DependencyInjection;

/// <summary>
/// Load secrets provided from Vault into the configuration.
/// </summary>
public static class VaultSecrets
{
    public static void LoadVaultSecrets(this WebApplicationBuilder builder)
    {
        // If running locally, load secrets from a local json file.
        if (builder.Environment.IsLocal())
        {
            // When calling this method you must have a secret.json file in your root.
            if (!File.Exists("secrets.json"))
            {
                throw new ConfigurationException($"The secrets.json file does not exists");
            }

            builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);

            return;
        }

        // Running remotely, load secrets from secrets folder.
        var folderPath = builder.Configuration.GetValue<string>("SecretPath") ??
                         throw new
                             ConfigurationException($"Configuration for key 'SecretPath' is missing");

        if (String.IsNullOrEmpty(folderPath))
            throw new ConfigurationException($"Configuration path for 'SecretPath' is empty");

        IEnumerable<string> files = Directory.EnumerateFiles(folderPath);

        if (!files.Any())
            throw new ConfigurationException($"No secret files to load from '{folderPath}'");

        foreach (string file in files)
        {
            string contents = File.ReadAllText(file).Trim();
            string fileName = file.Split("/").Last().Trim();
            builder.Configuration[fileName] = contents;
        }
    }
}