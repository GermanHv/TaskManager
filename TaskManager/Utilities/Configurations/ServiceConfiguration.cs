using TaskManager.Interfaces.Tasks;

namespace TaskManager.Utilities.Configurations
{
    public static class ServiceConfiguration
    {

        public static void AddServices(this IServiceCollection services)
        {
            services.AddScoped<ITaskService, TaskService>();
        }
    }
}