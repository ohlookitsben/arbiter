using System;

namespace Arbiter.MSBuild
{
    public class SolutionProject
    {
        public Guid Id { get; }
        public Guid TypeId { get; }
        public string Name { get; }
        public string FilePath { get; }

        public SolutionProject(Guid id, Guid typeId, string name, string path)
        {
            Id = id;
            TypeId = typeId;
            Name = name;
            FilePath = path;
        }
    }
}
