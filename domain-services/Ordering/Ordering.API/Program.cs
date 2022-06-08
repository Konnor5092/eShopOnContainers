namespace Ordering.API;

public class Program
{
    public static readonly string AppName = typeof(Program).Assembly.GetName().Name;
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        var app = builder.Build();

        app.UseRouting();

        app.Run();
    }
}