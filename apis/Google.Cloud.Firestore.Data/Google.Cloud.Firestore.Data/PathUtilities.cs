﻿// Copyright 2017, Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Google.Api.Gax;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Google.Cloud.Firestore.Data
{
    internal static class PathUtilities
    {
        private static readonly string s_generationDomain = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static readonly RandomNumberGenerator s_rng = RandomNumberGenerator.Create();
        private static readonly byte[] s_rngBuffer = new byte[30];

        private static readonly char[] s_slashSplit = new[] { '/' };
        /// <summary>
        /// Splits a path by slashes, and validates that no element is empty.
        /// </summary>
        /// <param name="path">Path to split. Must not be null.</param>
        /// <returns>An array of path elements, all of which are non-empty.</returns>
        internal static string[] SplitPath(string path)
        {
            GaxPreconditions.CheckNotNull(path, nameof(path));
            string[] elements = path.Split(s_slashSplit);
            if (elements.Contains(""))
            {
                throw new ArgumentException("Path cannot contain empty elements", nameof(path));
            }
            return elements;
        }

        internal static string ValidateId(string id, string paramName = "id")
        {
            GaxPreconditions.CheckNotNullOrEmpty(id, paramName);
            GaxPreconditions.CheckArgument(!id.Contains('/'), paramName, "ID cannot contain a '/' character.");
            return id;
        }

        /// <summary>
        /// Generates a random ID consisting of exactly 20 characters, each in the range a-z, A-Z, 0-9.
        /// </summary>
        internal static string GenerateId()
        {
            char[] chars = new char[20];
            lock (s_rngBuffer)
            {
                int charIndex = 0;
                while (charIndex < chars.Length)
                {
                    s_rng.GetBytes(s_rngBuffer);
                    for (int index = 0; index < s_rngBuffer.Length && charIndex < chars.Length; index++)
                    {
                        // Just take 6 bits from each generated byte. (The inefficiency here isn't worth
                        // the complex bit manipulation required to use all the bits.)
                        // Ignore any values which aren't in our range. We have 62 possible values,
                        // so almost everything will fix - it'll be very rare that we need to generate another
                        // set of bytes.
                        int sixBits = s_rngBuffer[index] & 0x3f;
                        if (sixBits < s_generationDomain.Length)
                        {
                            chars[charIndex++] = s_generationDomain[sixBits];
                        }
                    }
                }
                // TODO: Wipe s_rngBuffer? We're already creating a string which contains the data,
                // so any security risk would only be slightly mitigated.
            }
            return new string(chars);
        }
    }
}