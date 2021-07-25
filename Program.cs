using System;
using System.Threading.Tasks;

namespace MeanWords
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var controller = new Controller();
            await controller.Run();
        }
    }
}