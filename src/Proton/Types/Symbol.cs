/*
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Apache.Qpid.Proton.Buffer;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Apache.Qpid.Proton.Types
{
   public sealed class Symbol : IEquatable<Symbol>, IComparable, IComparable<Symbol>
   {
      private static readonly IDictionary<IProtonBuffer, Symbol> buffersToSymbols =
         new ConcurrentDictionary<IProtonBuffer, Symbol>();
      private static readonly IDictionary<string, Symbol> stringsToSymbols =
         new ConcurrentDictionary<string, Symbol>();

      private static readonly Symbol EMPTY_SYMBOL = new Symbol();

      // Prevents the Symbol cache from growing overly large if abused by creating overly
      // large Symbols which would all be stored in the symbol cache.
      private static readonly uint MAX_CACHED_SYMBOL_SIZE = 64;

      // Lazy allocated based on calls to stringify the given Symbol
      private string symbolString;

      private readonly IProtonBuffer underlying;
      private readonly int hashCode;

      private Symbol()
      {
         underlying = null; // TODO: Buffer allocator for empty buffer
         symbolString = "";
         hashCode = 32;
      }

      private Symbol(IProtonBuffer buffer)
      {
         underlying = buffer;
         hashCode = buffer.GetHashCode();
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching name to the string value provided.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol Lookup(string value)
      {
         if (value == null)
         {
            return null;
         }
         else if (value.Length == 0)
         {
            return EMPTY_SYMBOL;
         }
         else
         {
            Symbol symbol = null;

            if (!stringsToSymbols.TryGetValue(value, out symbol))
            {
               symbol = Lookup(ProtonByteBufferAllocator.INSTANCE.Wrap(Encoding.ASCII.GetBytes(value)));

               if (symbol.Length <= MAX_CACHED_SYMBOL_SIZE)
               {
                  // Try and keep the Symbol instance consistent with the one that is stored
                  // in the buffer to symbol dictionary.
                  stringsToSymbols[value] = symbol;
               }
            }

            return symbol;
         }
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching byte contents as the given buffer, if none exists a new Symbol
      /// is created using the given buffer which is not copied but used directly.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol Lookup(IProtonBuffer value)
      {
         return Lookup(value, false);
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching byte contents as the given buffer, if none exists a new Symbol
      /// is created using the given buffer which is not copied if the provided
      /// boolean option requests it.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <param name="copyOnCreate">should the given buffer be copied if a Symbol is created</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol Lookup(IProtonBuffer value, bool copyOnCreate)
      {
         if (value == null)
         {
            return null;
         }
         else if (!value.Readable)
         {
            return EMPTY_SYMBOL;
         }
         else
         {
            Symbol symbol = null;

            if (!buffersToSymbols.TryGetValue(value, out symbol))
            {
               if (copyOnCreate)
               {
                  long symbolSize = value.ReadableBytes;
                  IProtonBuffer copy = ProtonByteBufferAllocator.INSTANCE.Allocate(symbolSize, symbolSize);
                  value.CopyInto(value.ReadOffset, copy, 0, symbolSize);
                  copy.WriteOffset = symbolSize;
                  value = copy;
               }

               symbol = new Symbol(value);

               if (symbol.Length <= MAX_CACHED_SYMBOL_SIZE)
               {
                  if (!buffersToSymbols.TryAdd(value, symbol))
                  {
                     symbol = buffersToSymbols[value];
                  }
               }
            }

            return symbol;
         }
      }

      /// <summary>
      /// Returns the number of ASCII characters that comprise this Symbol
      /// </summary>
      public int Length
      {
         get { return (int) underlying.ReadableBytes; }
      }

      /// <summary>
      /// Writes a copy of the Symbol bytes to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the Symbol bytes to</param>
      public void WriteTo(IProtonBuffer buffer)
      {
         buffer.EnsureWritable(Length);
         underlying.CopyInto(underlying.ReadOffset, buffer, buffer.WriteOffset, underlying.ReadableBytes);
      }

      public override string ToString()
      {
         if (symbolString == null && underlying.Readable)
         {
            symbolString = underlying.ToString(Encoding.ASCII);
            if (symbolString.Length <= MAX_CACHED_SYMBOL_SIZE)
            {
               if (!stringsToSymbols.TryAdd(symbolString, this))
               {
                  symbolString = stringsToSymbols[symbolString].symbolString;
               }
            }
         }

         return symbolString ?? "";
      }

      public override int GetHashCode()
      {
         return hashCode;
      }

      public override bool Equals(object symbol)
      {
         if (symbol == null || symbol.GetType() != GetType())
         {
            return false;
         }

         return Equals(symbol as Symbol);
      }

      public bool Equals(Symbol symbol)
      {
         if (symbol == null)
         {
            return false;
         }

         return underlying.Equals(symbol.underlying);
      }

      public int CompareTo(Symbol other)
      {
         return underlying.CompareTo(other.underlying);
      }

      public int CompareTo(object other)
      {
         return CompareTo(other as Symbol);
      }
   }
}