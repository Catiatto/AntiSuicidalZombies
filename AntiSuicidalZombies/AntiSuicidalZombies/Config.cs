using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using CustomPlayerEffects;

namespace AntiSuicidalZombies
{
    public class Config
    {
        [Description("Should debugging logs be enabled?")]
        public bool Debug { get; set; } = false;

        [Description("Effect(s) applied to zombies after walking into a tesla and their duration time.")]
        public Dictionary<string, EffectParameters> TeslaEffects { get; set; } = new Dictionary<string, EffectParameters>
        {
            {
               "Blurred",
               new EffectParameters
               {
                   Intensity = 1,
                   Duration = 10f
               }
            },
            {
               "Burned",
               new EffectParameters
               {
                   Intensity = 1,
                   Duration = 30f
               }
            }
        };

        [Description("Effect(s) applied to zombies after being crushed by a door or an elevator and their duration time.")]
        public Dictionary<string, EffectParameters> CrushedEffects { get; set; } = new Dictionary<string, EffectParameters>
        {
            {
               "Concussed",
               new EffectParameters
               {
                   Intensity = 5,
                   Duration = 10f
               }
            },
            {
               "Deafened",
               new EffectParameters
               {
                   Intensity = 1,
                   Duration = 20f
               }
            },
            {
               "Slowness",
               new EffectParameters
               {
                   Intensity = 30,
                   Duration = 30f
               }
            }
        };
    }

    public class EffectParameters
    {
        public byte Intensity { get; set; }

        public float Duration { get; set; }
    }
}
