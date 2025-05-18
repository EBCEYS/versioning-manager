using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EBCEYS.Environment_Replacer
{
    public class Program
    {
        const string entryStart = "${";
        const string entryEnd = "}";
        /// <summary>
        /// Make a copy of <paramref name="source"/> with replaced environment variables.<br/>
        /// Example of environment variable: <c>${MY_ENVIRONMENT_VALUE}<c/>
        /// </summary>
        public static async Task Main(string[] args)
        {
            FileInfo source = new(args[0]);
            FileInfo dest = new(args[1]);
            bool overwrite = true;
            string defaultValue = string.Empty;
            int indexOfOverwrite = Array.IndexOf(args, "-w");
            if (indexOfOverwrite > 1)
            {
                overwrite = bool.Parse(args[indexOfOverwrite + 1]);
            }
            int indexOfDefaultValue = Array.IndexOf(args, "-d");
            if (indexOfDefaultValue > 1)
            {
                defaultValue = args[indexOfDefaultValue + 1];
            }
            if (!source.Exists)
            {
                throw new FileNotFoundException("Source file does not exists!", source.FullName);
            }
            if (dest.Exists && !overwrite)
            {
                throw new InvalidOperationException($"Destination file {dest.FullName} exists but overwrite params set false!");
            }
            using StreamReader sr = source.OpenText();
            FileInfo tmpDest = new(dest.FullName + "rep-tmp");
            using StreamWriter sw = tmpDest.CreateText();
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                StringBuilder newLineBuilder = new();
                string tmpLine = new(line);
                for (int startIndex = tmpLine.IndexOf(entryStart); ; startIndex = tmpLine.IndexOf(entryStart))
                {
                    int nextIndex = 0;
                    if (startIndex == -1)
                    {
                        newLineBuilder.Append(tmpLine);
                        break;
                    }
                    int endIndex = tmpLine.IndexOf(entryEnd);
                    if (endIndex == -1)
                    {
                        nextIndex = startIndex + entryStart.Length;
                        tmpLine = tmpLine[nextIndex..];
                        newLineBuilder.Append(tmpLine);
                        continue;
                    }
                    nextIndex = endIndex;
                    string envName = tmpLine[(startIndex + entryStart.Length)..nextIndex];
                    string envValue = Environment.GetEnvironmentVariable(envName) ?? defaultValue;

                    StringBuilder sb = new();
                    sb.Append(entryStart);
                    sb.Append(envName);
                    sb.Append(entryEnd);

                    // tmpLine = tmpLine.Replace(sb.ToString(), envValue);
                    nextIndex += entryEnd.Length;
                    string appendLine = tmpLine[..nextIndex].Replace(sb.ToString(), envValue);
                    newLineBuilder.Append(appendLine);
                    tmpLine = tmpLine[nextIndex..];
                }
                await sw.WriteLineAsync(newLineBuilder.ToString());
            }
            tmpDest.MoveTo(dest.FullName, overwrite);
        }
    }
}