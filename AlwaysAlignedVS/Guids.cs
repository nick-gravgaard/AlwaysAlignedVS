// Guids.cs
// MUST match guids.h
using System;

namespace AlwaysAligned
{
    static class GuidList
    {
        public const string guidAlwaysAlignedPkgString = "c982f983-3dce-4114-91c0-e534dd039dda";
		public const string guidAlwaysAlignedCmdSetString = "234580c4-8a2c-4ae1-8e4f-5bc708b188fe";

		public static readonly Guid guidAlwaysAlignedCmdSet = new Guid(guidAlwaysAlignedCmdSetString);
    };
}