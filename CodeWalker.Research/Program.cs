using CodeWalker.GameFiles.DataCollectors;

namespace CodeWalker.Research
{
    public class Program
    {
        static void Main(string[] args)
        {
            var dataCollector = new BoneTransformsDataCollector();
            dataCollector.Start();
        }
    }
}
