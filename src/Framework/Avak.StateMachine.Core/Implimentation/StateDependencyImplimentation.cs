namespace Avak.StateMachine.Core.Implimentation
{
    public static class StateDependencyImplimentation
    {
        // For more undertanding of the following look at the sample WpfViewChanged
        //public static List<object?>? StateDependencyObjectFinderDefaultImplimentation(Type stateType)
        //{
        //    List<object?>? dependencies = [];
        //    return dependencies;
        //}

        public static List<Type?>? StateDependencyTypeFinderDefaultImplimentation(Type stateType)
        {
            List<Type?>? dependencies = [];
            return dependencies;
        }

        /// <summary>
        /// Given a states dependency type, this delegate resolves that type into a concrete instance. 
        /// The default implimentation simply returns null. This is default implimentation is primarly used for testing.
        /// </summary>
        /// <param name="stateDependencyType"></param>
        /// <returns></returns>
        public static object? StateDependencyResolverDefaultImplimentation(Type stateDependencyType)
        {
            object? dependencies = null;
            return dependencies;
        }
    }
}
