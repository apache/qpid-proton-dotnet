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

using System;

namespace Apache.Qpid.Proton.Test.Driver.Matchers.Types
{
   /// <summary>
   /// A set of utility methods for working with .NET numeric types
   /// </summary>
   public static class Numbers
   {
      /// <summary>
      /// Returns true if the value provided is number type.
      /// </summary>
      /// <param name="value">The value to check</param>
      /// <returns>true if the value is a number type</returns>
      public static bool IsNumericType(object value)
      {
         return IsFloatingPointNumeric(value) || IsFixedPointNumeric(value);
      }

      /// <summary>
      /// Returns true if the value is a number and is a floating point type.
      /// </summary>
      /// <param name="value">The object to check</param>
      /// <returns>true if the object is a floating point numeric type</returns>
      public static bool IsFloatingPointNumeric(object value)
      {
         if (null != value)
         {
            if (value is double) return true;
            if (value is float) return true;
            if (value is decimal) return true;
         }

         return false;
      }

      /// <summary>
      /// Return true if the value is a number and is not a floating point type
      /// </summary>
      /// <param name="value">The object to check</param>
      /// <returns>true if the object is a fixed point numeric type</returns>
      public static bool IsFixedPointNumeric(object value)
      {
         if (null != value)
         {
            if (value is byte) return true;
            if (value is sbyte) return true;
            if (value is int) return true;
            if (value is uint) return true;
            if (value is long) return true;
            if (value is ulong) return true;
            if (value is short) return true;
            if (value is ushort) return true;
            if (value is char) return true;
         }
         return false;
      }

      /// <summary>
      /// Compares two values and determines if they are numerically equivalent.
      /// </summary>
      /// <param name="lhs"></param>
      /// <param name="rhs"></param>
      /// <returns>true if the values are equal</returns>
      internal static bool AreEqual(object lhs, object rhs)
      {
         if (!IsNumericType(lhs) && !IsNumericType(rhs))
         {
            return false;
         }

         if (lhs is double || rhs is double)
         {
            return AreEqual(Convert.ToDouble(lhs), Convert.ToDouble(rhs));
         }
         else if (lhs is float || rhs is float)
         {
            return AreEqual(Convert.ToSingle(lhs), Convert.ToSingle(rhs));
         }
         else if (lhs is decimal || rhs is decimal)
         {
            return AreEqual(Convert.ToDecimal(lhs), Convert.ToDecimal(rhs));
         }
         else if (lhs is ulong || rhs is ulong)
         {
            return AreEqual(Convert.ToUInt64(lhs), Convert.ToUInt64(rhs));
         }
         else if (lhs is long || rhs is long)
         {
            return AreEqual(Convert.ToInt64(lhs), Convert.ToInt64(rhs));
         }
         else if (lhs is uint || rhs is uint)
         {
            return AreEqual(Convert.ToUInt32(lhs), Convert.ToUInt32(rhs));
         }
         else if (lhs is int || rhs is int)
         {
            return AreEqual(Convert.ToInt32(lhs), Convert.ToInt32(rhs));
         }
         else if (lhs is ushort || rhs is ushort)
         {
            return AreEqual(Convert.ToUInt16(lhs), Convert.ToUInt16(rhs));
         }
         else if (lhs is short || rhs is short)
         {
            return AreEqual(Convert.ToInt16(lhs), Convert.ToInt16(rhs));
         }
         else if (lhs is byte || rhs is byte)
         {
            return AreEqual(Convert.ToByte(lhs), Convert.ToByte(rhs));
         }
         else if (lhs is sbyte || rhs is sbyte)
         {
            return AreEqual(Convert.ToSByte(lhs), Convert.ToSByte(rhs));
         }

         return false;
      }

      internal static bool AreEqual(double lhs, double rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(float lhs, float rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(decimal lhs, decimal rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(long lhs, long rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(ulong lhs, ulong rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(uint lhs, uint rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(int lhs, int rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(ushort lhs, ushort rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(short lhs, short rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(byte lhs, byte rhs)
      {
         return lhs.Equals(rhs);
      }

      internal static bool AreEqual(sbyte lhs, sbyte rhs)
      {
         return lhs.Equals(rhs);
      }
   }
}
