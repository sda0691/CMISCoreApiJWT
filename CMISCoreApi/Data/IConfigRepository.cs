using CMISCoreApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMISCoreApi.Data
{
    public interface IConfigRepository
    {
        Task<Config> GetConfigByCategoryConfigName(string category, string configname);
    }
}