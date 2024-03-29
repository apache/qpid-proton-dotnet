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
using System.Collections.Generic;
using Apache.Qpid.Proton.Types;
using Apache.Qpid.Proton.Types.Messaging;
using Apache.Qpid.Proton.Types.Transactions;

namespace Apache.Qpid.Proton.Client.Implementation
{
   /// <summary>
   /// Client implementation of a delivery state mapping to the proton types
   /// </summary>
   public abstract class ClientDeliveryState : IDeliveryState
   {
      public virtual bool IsAccepted => Type == DeliveryStateType.Accepted;

      public abstract DeliveryStateType Type { get; }

      public abstract Types.Transport.IDeliveryState ProtonDeliveryState { get; }

   }

   /// <summary>
   /// Client version of the proton Accepted delivery state
   /// </summary>
   public sealed class ClientAccepted : ClientDeliveryState
   {
      public static readonly ClientAccepted Instance = new();

      private ClientAccepted() { }

      public override DeliveryStateType Type => DeliveryStateType.Accepted;

      public override Types.Transport.IDeliveryState ProtonDeliveryState => Accepted.Instance;

   }

   /// <summary>
   /// Client version of the proton Released delivery state
   /// </summary>
   public sealed class ClientReleased : ClientDeliveryState
   {
      public static readonly ClientReleased Instance = new();

      private ClientReleased() { }

      public override DeliveryStateType Type => DeliveryStateType.Released;

      public override Types.Transport.IDeliveryState ProtonDeliveryState => Released.Instance;

   }

   /// <summary>
   /// Client version of the proton Rejected delivery state
   /// </summary>
   public sealed class ClientRejected : ClientDeliveryState
   {
      private readonly Rejected rejected = new();

      internal ClientRejected(Rejected rejected)
      {
         this.rejected.Error = rejected.Error?.Copy();
      }

      /// <summary>
      /// Create a new Rejected client delivery state with the provided information.
      /// </summary>
      /// <param name="condition">The condition value to convey to the remote</param>
      /// <param name="description">The description value to convey to the remote</param>
      public ClientRejected(string condition, string description)
      {
         this.rejected.Error = new Types.Transport.ErrorCondition(condition, description);
      }

      /// <summary>
      /// Create a new Rejected client delivery state with the provided information.
      /// </summary>
      /// <param name="condition">The condition value to convey to the remote</param>
      /// <param name="description">The description value to convey to the remote</param>
      /// <param name="info">The information map value to convey to the remote</param>
      public ClientRejected(string condition, string description, IDictionary<string, object> info)
      {
         if (condition != null || description != null)
         {
            rejected.Error = new Types.Transport.ErrorCondition(
                Symbol.Lookup(condition), description, ClientConversionSupport.ToSymbolKeyedMap(info));
         }
      }

      public override DeliveryStateType Type => DeliveryStateType.Rejected;

      public override Types.Transport.IDeliveryState ProtonDeliveryState => rejected;

   }

   /// <summary>
   /// Client version of the proton Modified delivery state
   /// </summary>
   public sealed class ClientModified : ClientDeliveryState
   {
      private readonly Modified modified = new();

      internal ClientModified(Modified modified)
      {
         this.modified.DeliveryFailed = modified.DeliveryFailed;
         this.modified.UndeliverableHere = modified.UndeliverableHere;
         this.modified.MessageAnnotations = new Dictionary<Symbol, object>(modified.MessageAnnotations);
      }

      /// <summary>
      /// Create a new instance with the given outcome values.
      /// </summary>
      /// <param name="failed">did the delivery fail for some reason</param>
      /// <param name="undeliverable">should the delivery be treated an not deliverable here any longer</param>
      public ClientModified(bool failed, bool undeliverable)
      {
         modified.DeliveryFailed = failed;
         modified.UndeliverableHere = undeliverable;
      }

      /// <summary>
      /// Create a new instance with the given outcome values.
      /// </summary>
      /// <param name="failed">did the delivery fail for some reason</param>
      /// <param name="undeliverable">should the delivery be treated an not deliverable here any longer</param>
      /// <param name="annotations">modification to existing message annotations</param>
      public ClientModified(bool failed, bool undeliverable, IDictionary<string, object> annotations)
      {
         modified.DeliveryFailed = failed;
         modified.UndeliverableHere = undeliverable;
         modified.MessageAnnotations = ClientConversionSupport.ToSymbolKeyedMap(annotations);
      }

      public override DeliveryStateType Type => DeliveryStateType.Modified;

      public override Types.Transport.IDeliveryState ProtonDeliveryState => modified;

   }

   /// <summary>
   /// Client version of the proton Transactional delivery state
   /// </summary>
   public sealed class ClientTransactional : ClientDeliveryState
   {
      private readonly TransactionalState txnState = new();

      internal ClientTransactional(TransactionalState txnState)
      {
         this.txnState.Outcome = txnState.Outcome;
         this.txnState.TxnId = txnState.TxnId.Copy();
      }

      public override bool IsAccepted => txnState.Outcome is Accepted;

      public override DeliveryStateType Type => DeliveryStateType.Transactional;

      public override Types.Transport.IDeliveryState ProtonDeliveryState => txnState;

   }

   #region Extension types for Proton and Client delivery state types

   internal static class DeliveryStateExtensions
   {
      public static IDeliveryState ToClientDeliveryState(this IOutcome outcome)
      {
         if (outcome == null)
         {
            return null;
         }

         if (outcome is Accepted)
         {
            return ClientAccepted.Instance;
         }
         else if (outcome is Released)
         {
            return ClientReleased.Instance;
         }
         else if (outcome is Rejected rejected)
         {
            return new ClientRejected(rejected);
         }
         else if (outcome is Modified modified)
         {
            return new ClientModified(modified);
         }

         throw new ArgumentException("Cannot map to unknown Proton Outcome to a client delivery state: " + outcome);
      }

      public static IDeliveryState ToClientDeliveryState(this Types.Transport.IDeliveryState deliveryState)
      {
         if (deliveryState == null)
         {
            return null;
         }

         if (deliveryState is Accepted)
         {
            return ClientAccepted.Instance;
         }
         else if (deliveryState is Released)
         {
            return ClientReleased.Instance;
         }
         else if (deliveryState is Rejected rejected)
         {
            return new ClientRejected(rejected);
         }
         else if (deliveryState is Modified modified)
         {
            return new ClientModified(modified);
         }
         else if (deliveryState is TransactionalState state)
         {
            return new ClientTransactional(state);
         }

         throw new ArgumentException("Cannot map to unknown Proton delivery state to a client delivery state: " + deliveryState);
      }

      public static Types.Transport.IDeliveryState AsProtonType(this IDeliveryState state)
      {
         if (state == null)
         {
            return null;
         }
         else if (state is ClientDeliveryState state1)
         {
            return state1.ProtonDeliveryState;
         }
         else
         {
            return state.Type switch
            {
               DeliveryStateType.Accepted => Accepted.Instance,
               DeliveryStateType.Released => Released.Instance,
               DeliveryStateType.Rejected => new Rejected(),// TODO - How do we aggregate the different values into one DeliveryState Object
               DeliveryStateType.Modified => new Modified(),// TODO - How do we aggregate the different values into one DeliveryState Object
               DeliveryStateType.Transactional => throw new ArgumentException("Cannot manually enlist delivery in AMQP Transactions"),
               _ => throw new InvalidOperationException("Client does not support the given Delivery State type: " + state.Type),
            };
         }
      }

      public static DeliveryStateType ToDeliveryStateType(this Symbol outcome)
      {
         try
         {
            return Enum.Parse<DeliveryStateType>(outcome?.ToString(), true);
         }
         catch (Exception)
         {
            throw new ArgumentException("Cannot map outcome name to unknown Proton DeliveryState.Type");
         }
      }

      public static Symbol ToSymbolicType(this DeliveryStateType type)
      {
         return Symbol.Lookup(type.ToString().ToUpper());
      }
   }

   #endregion
}