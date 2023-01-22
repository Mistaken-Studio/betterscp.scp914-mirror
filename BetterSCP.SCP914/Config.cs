using System.ComponentModel;

namespace Mistaken.BetterSCP.SCP914
{
    internal sealed class Config
    {
        [Description("If true then debug will be displayed")]
        public bool Debug { get; set; } = false;

        [Description("Scp914 damage values")]
        public float DamageOnCoarse { get; set; } = 250;

        public float DamageOnRough { get; set; } = 500;
    }
}
