using Avak.StateMachine.Core.Contracts;
using System.Reflection;

namespace Avak.StateMachine.Core.Implimentation
{
    public class CurrentAppDomainTypeFinder : ITypeFinder
    {
        private readonly Dictionary<string, Type> typeCache = [];

        private int numberOfTries = 0;

        /// <summary>
        /// Tries to find a type object in the current app domain, given the type name along with its namespace
        /// </summary>
        /// <param name="nameSpace">The namespace of the type</param>
        /// <param name="typeName">The class name or type name</param>
        /// <param name="type">The type object</param>
        /// <returns>True if the type is found, else false.</returns>
        public bool TryFindType(string nameSpace, string typeName, out Type type)
        {
            string typeFullName = nameSpace + "." + typeName;
            lock (typeCache)
            {
                if (!typeCache.TryGetValue(typeFullName, out type!))
                {
                    type = FindTypeInAssembliesInCurrentAppDomain(typeFullName);
                    if (type != null)
                    {
                        typeCache[typeFullName] = type;
                    }
                }
            }
            return type != null;
        }

        private Type FindTypeInAssembliesInCurrentAppDomain(string typeName)
        {
            Type typeFound = null!;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                typeFound = assembly.GetType(typeName)!;
                if (typeFound != null)
                    break;
            }

            if (typeFound == null && numberOfTries < 2)
            {
                numberOfTries++;
                Thread.Sleep(100);
                typeFound = FindTypeInAssembliesInCurrentAppDomain(typeName);
            }
            else
            {
                numberOfTries = 0;
            }

            return typeFound!;
        }
    }
}
