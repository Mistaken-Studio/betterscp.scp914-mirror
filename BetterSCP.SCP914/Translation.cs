// -----------------------------------------------------------------------
// <copyright file="Translation.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Exiled.API.Interfaces;

namespace Mistaken.BetterSCP.SCP914
{
    internal class Translation : ITranslation
    {
        public string Scp914_coarse { get; set; } = "Killed by high heat";

        public string Scp914_rough { get; set; } = "Killed by extreme heat";
    }
}
