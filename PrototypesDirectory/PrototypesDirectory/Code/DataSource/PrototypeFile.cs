using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Matematici.DataSource
{
    public class PrototypeFile
    {
        public PrototypeFile(string FileName)
        {
            this.FileName = FileName;

            var Tokens = FileName.Split('#');
            if (Tokens.Length != 6)
                throw new Exception($"Invalid file name: {FileName}, invalid number of tokens.");

            Company = Tokens[0];
            if (string.IsNullOrWhiteSpace(Company))
                throw new Exception($"Invalid file name: {FileName}, invalid company.");

            Department = Tokens[1];
            if (string.IsNullOrWhiteSpace(Department))
                throw new Exception($"Invalid file name: {FileName}, invalid department.");

            PrototypeType = Tokens[2];
            if (string.IsNullOrWhiteSpace(PrototypeType))
                throw new Exception($"Invalid file name: {FileName}, invalid prototype type.");

            Code = Tokens[3];
            if (string.IsNullOrWhiteSpace(Code))
                throw new Exception($"Invalid file name: {FileName}, invalid code.");

            int Version;
            if (!int.TryParse(Tokens[4], out Version))
                throw new Exception($"Invalid file name: {FileName}, invalid version.");
            this.Version = Version;

            Description = Tokens[5];
            if (string.IsNullOrWhiteSpace(Description))
                throw new Exception($"Invalid file name: {FileName}, invalid description.");
        }

        public string FileName { get; }

        public string Company { get; }

        public string Department { get; }

        public string PrototypeType { get; }

        public string Code { get; }

        public int Version { get; }

        public string Description { get; }

    }

}