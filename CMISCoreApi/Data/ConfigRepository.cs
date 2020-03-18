using CMISCoreApi.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMISCoreApi.Data
{
    public class ConfigRepository: IConfigRepository
    {
        private readonly DataContext _context;

        public ConfigRepository(DataContext context)
        {
 
            this._context = context;
        }

        public async Task<Config> GetConfigByCategoryConfigName(string category, string configname)
        {
            var config = await _context.Configs.FirstOrDefaultAsync(p => p.Category == category && p.ConfigName == configname) ;

            return config;
        }

    }
}
