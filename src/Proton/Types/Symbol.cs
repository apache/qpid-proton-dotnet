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
using System.Collections.Concurrent;

namespace Apache.Qpid.Proton.Types
{
   public sealed class Symbol : IEquatable<Symbol>, IComparable, IComparable<Symbol>
   {
      // Prevents the Symbol cache from growing overly large if abused by creating overly
      // large Symbols which would all be stored in the symbol cache. The SASL symbol cache
      // is kept considerably smaller since there shouldn't be many entries used for the
      // mechanisms and descriptor symbols as compared to the main application level cache.

      private static readonly uint MaxSymbolCacheEntries = 8192;
      private static readonly uint MaxCachedSymbolSize = 64;

      private static readonly uint MaxSaslSymbolCacheEntries = 128;
      private static readonly uint MaxCachedSaslSymbolSize = 32;

      private static readonly SymbolCache CachedSymbols = new SymbolCache(MaxSymbolCacheEntries, MaxCachedSymbolSize);
      private static readonly SymbolCache CachedSaslSymbols = new SymbolCache(MaxSaslSymbolCacheEntries, MaxCachedSaslSymbolSize);

      private static readonly Symbol EMPTY_SYMBOL = new();

      // Lazy allocated based on calls to stringify the given Symbol
      private string symbolString;

      private readonly IProtonBuffer underlying;
      private readonly int hashCode;
      private readonly SymbolCache symbolCache;

      private Symbol()
      {
         underlying = ProtonByteBufferAllocator.Instance.Allocate(0, 0);
         symbolString = "";
         hashCode = 32;
         symbolCache = null;
      }

      private Symbol(IProtonBuffer buffer, SymbolCache cache)
      {
         underlying = buffer;
         hashCode = buffer.GetHashCode();
         symbolCache = cache;
      }

      /// <summary>
      /// Allows a string value to be implicitly converted to a Symbol
      /// </summary>
      /// <param name="symbolString">The String to convert</param>
      public static implicit operator Symbol(string symbolString) => Lookup(symbolString);

      /// <summary>
      /// Allows a Symbol object to be implicitly converted to a string value.
      /// </summary>
      /// <param name="value">The Symbol to convert</param>
      public static implicit operator string(Symbol value) => value?.ToString();

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
            return CachedSymbols.Lookup(value);
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
         else if (!value.IsReadable)
         {
            return EMPTY_SYMBOL;
         }
         else
         {
            return CachedSymbols.Lookup(value, copyOnCreate);
         }
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching name to the string value provided. This method looks in the
      /// smaller SASL symbol cache for the matching Symbol value.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol SaslLookup(string value)
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
            return CachedSaslSymbols.Lookup(value);
         }
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching byte contents as the given buffer, if none exists a new Symbol
      /// is created using the given buffer which is not copied but used directly.
      /// This method looks in the smaller SASL Symbol cache for the matching value.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol SaslLookup(IProtonBuffer value)
      {
         return SaslLookup(value, false);
      }

      /// <summary>
      /// Lookup or create a singleton instance of the given Symbol that has the
      /// matching byte contents as the given buffer, if none exists a new Symbol
      /// is created using the given buffer which is not copied if the provided
      /// boolean option requests it. This method looks in the smaller SASL Symbol
      /// cache for the matching value.
      /// </summary>
      /// <param name="value">the stringified symbol name</param>
      /// <param name="copyOnCreate">should the given buffer be copied if a Symbol is created</param>
      /// <returns>A singleton instance of the named Symbol</returns>
      public static Symbol SaslLookup(IProtonBuffer value, bool copyOnCreate)
      {
         if (value == null)
         {
            return null;
         }
         else if (!value.IsReadable)
         {
            return EMPTY_SYMBOL;
         }
         else
         {
            return CachedSaslSymbols.Lookup(value, copyOnCreate);
         }
      }

      /// <summary>
      /// Returns the number of ASCII characters that comprise this Symbol
      /// </summary>
      public int Length
      {
         get { return (int)underlying.ReadableBytes; }
      }

      /// <summary>
      /// Writes a copy of the Symbol bytes to the given buffer.
      /// </summary>
      /// <param name="buffer">The buffer to write the Symbol bytes to</param>
      public void WriteTo(IProtonBuffer buffer)
      {
         buffer.EnsureWritable(Length);
         underlying.CopyInto(underlying.ReadOffset, buffer, buffer.WriteOffset, underlying.ReadableBytes);
         buffer.WriteOffset += Length;
      }

      public override string ToString()
      {
         if (symbolString == null && underlying.IsReadable)
         {
            symbolCache.ToString(this);
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

      private static Symbol CreateSymbol(SymbolCache cache, IProtonBuffer buffer, bool copyOnCreate)
      {
            if (copyOnCreate)
            {
               long symbolSize = buffer.ReadableBytes;
               IProtonBuffer copy = ProtonByteBufferAllocator.Instance.Allocate(symbolSize, symbolSize);
               buffer.CopyInto(buffer.ReadOffset, copy, 0, symbolSize);
               copy.WriteOffset = symbolSize;
               buffer = copy;
            }

            return new Symbol(buffer, cache);
      }

      private sealed class SymbolCache
      {
         private readonly ConcurrentDictionary<IProtonBuffer, Symbol> buffersToSymbols = new();
         private readonly ConcurrentDictionary<string, Symbol> stringsToSymbols = new();

         private readonly uint maxCachedSymbols;
         private readonly uint maxCachedSymbolSize;

         public SymbolCache(uint maxCachedSymbols, uint maxCachedSymbolSize)
         {
            this.maxCachedSymbols = maxCachedSymbols;
            this.maxCachedSymbolSize = maxCachedSymbolSize;
         }

         public Symbol Lookup(string value)
         {
            if (!stringsToSymbols.TryGetValue(value, out Symbol symbol))
            {
               symbol = Lookup(ProtonByteBufferAllocator.Instance.Wrap(Encoding.ASCII.GetBytes(value)));

               if (symbol.symbolString == null)
               {
                  // In case of a new Symbol object being created we want to ensure the String value
                  // is loaded now since we know what it is and future calls won't need to access the
                  // cache for no reason.
                  symbol.symbolString = value;
               }

               if (symbol.Length <= maxCachedSymbolSize && stringsToSymbols.Count < maxCachedSymbols)
               {
                  // Try and keep the Symbol instance consistent with the one that is stored
                  // in the buffer to symbol dictionary.
                  stringsToSymbols[value] = symbol;
               }
            }

            return symbol;
         }

         public Symbol Lookup(IProtonBuffer value)
         {
            return Lookup(value, false);
         }

         public Symbol Lookup(IProtonBuffer value, bool copyOnCreate)
         {
            bool canCache = value.ReadableBytes <= maxCachedSymbolSize;

            if (canCache)
            {
               if (!buffersToSymbols.TryGetValue(value, out Symbol symbol))
               {
                  // Lock to prevent the cache from possibly growing beyond the max cache size
                  // due to races on early fills from multiple connection threads.
                  lock (EMPTY_SYMBOL)
                  {
                     if (!buffersToSymbols.TryGetValue(value, out symbol))
                     {
                        symbol = CreateSymbol(this, value, copyOnCreate);

                        if (buffersToSymbols.Count < maxCachedSymbols)
                        {
                           if (!buffersToSymbols.TryAdd(value, symbol))
                           {
                              symbol = buffersToSymbols[value];
                           }
                        }
                     }
                  }
               }

               return symbol;
            }
            else
            {
               return CreateSymbol(this, value, copyOnCreate);
            }
         }

         public string ToString(Symbol symbol)
         {
            if (symbol.symbolString == null && symbol.underlying.IsReadable)
            {
               symbol.symbolString = symbol.underlying.ToString(Encoding.ASCII);

               if (symbol.symbolString.Length <= maxCachedSymbolSize && stringsToSymbols.Count < maxCachedSymbols)
               {
                  if (!stringsToSymbols.TryAdd(symbol.symbolString, symbol))
                  {
                     symbol.symbolString = stringsToSymbols[symbol.symbolString].symbolString;
                  }
               }
            }

            return symbol.symbolString ?? "";
         }
      }
   }
}