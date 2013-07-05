using System;
using Autofac;
using Autofac.Configuration;

namespace StockNotification.Database.Interface
{
    public sealed class ApplicationEntry : IServiceProvider
    {
        private ApplicationEntry()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader("IoC"));
            Container = builder.Build();
        }

        private IContainer Container
        {
            get;
            set;
        }

        private static object LockObj = new object();
        private static ApplicationEntry AppEntry = null;

        /// <summary>
        /// 得到应用程序服务的实例
        /// </summary>
        public static ApplicationEntry Instance
        {
            get
            {
                if (AppEntry == null)
                {
                    lock (LockObj)
                    {
                        if (AppEntry == null)
                        {
                            AppEntry = new ApplicationEntry();
                        }
                    }
                }
                return AppEntry;
            }
        }

        /// <summary>
        /// 尝试获取指定类型的服务对象
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="_instance">服务实例</param>
        /// <returns></returns>
        public bool TryGetService(Type serviceType, out object _instance)
        {
            return this.Container.TryResolve(serviceType, out _instance);
        }

        /// <summary>
        /// 尝试获取指定类型的服务对象
        /// </summary>
        /// <typeparam name="T">服务类型</typeparam>
        /// <param name="_outObject">服务实例</param>
        /// <returns></returns>
        public bool TryGetService<T>(out T _outObject)
        {
            return this.Container.TryResolve<T>(out _outObject);
        }

        /// <summary>
        /// 获取指定类型的服务对象。
        /// </summary>
        /// <param name="serviceType">一个对象，它指定要获取的服务对象的类型。</param>
        /// <returns><paramref name="serviceType"/> 类型的服务对象。- 或 -如果没有 <paramref name="serviceType"/> 类型的服务对象，则为 null。</returns>
        /// <remarks></remarks>
        public object GetService(Type serviceType)
        {
            return this.Container.Resolve(serviceType);
        }

        /// <summary>
        /// 获取指定类型的服务对象
        /// </summary>
        /// <typeparam name="T">服务对象类型</typeparam>
        /// <returns></returns>
        public T GetService<T>()
        {
            return this.Container.Resolve<T>();
        }
    }
}
