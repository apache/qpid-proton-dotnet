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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Apache.Qpid.Proton.Test.Driver.Codec.Transport;
using Apache.Qpid.Proton.Test.Driver.Exceptions;

namespace Apache.Qpid.Proton.Test.Driver
{
   /// <summary>
   /// Tracks information related to an opened Session and its various links
   /// </summary>
   public sealed class SessionTracker
   {
      private readonly IDictionary<string, LinkTracker> senderByNameMap = new Dictionary<string, LinkTracker>();
      private readonly IDictionary<string, LinkTracker> receiverByNameMap = new Dictionary<string, LinkTracker>();

      private readonly OrderedDictionary localLinks = new OrderedDictionary();
      private readonly OrderedDictionary remoteLinks = new OrderedDictionary();

      private ushort? localChannel;
      private ushort? remoteChannel;
      private uint? nextIncomingId;
      private uint? nextOutgoingId;
      private Begin remoteBegin;
      private Begin localBegin;
      private End remoteEnd;
      private End localEnd;

      private readonly AMQPTestDriver driver;

      public SessionTracker(AMQPTestDriver driver)
      {
         this.driver = driver;
      }

      public AMQPTestDriver Driver => driver;

      public End RemoteEnd => remoteEnd;

      public End LocalEnd => localEnd;

      public Begin RemoteBegin => remoteBegin;

      public Begin LocalBegin => localBegin;

      public ushort? RemoteChannel => remoteChannel;

      public ushort? LocalChannel => localChannel;

      public uint? NextIncomingId => nextIncomingId;

      public uint? NextOutgoingId => nextOutgoingId;

      public LinkTracker LastOpenedLink
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = localLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
            }

            return linkTracker;
         }
      }

      public LinkTracker LastRemotelyOpenedLink
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = remoteLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
            }

            return linkTracker;
         }
      }

      public LinkTracker LastOpenedCoordinatorLink
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = localLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.Coordinator == null)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public LinkTracker LastRemotelyOpenedCoordinatorLink
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = remoteLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.Coordinator == null)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public LinkTracker LastRemotelyOpenedSender
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = remoteLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.IsReceiver)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public LinkTracker LastRemotelyOpenedReceiver
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = remoteLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.IsSender)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public LinkTracker LastOpenedSender
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = localLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.IsReceiver)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public LinkTracker LastOpenedReceiver
      {
         get
         {
            LinkTracker linkTracker = null;
            IDictionaryEnumerator enumerator = localLinks.GetEnumerator();
            while (enumerator.MoveNext())
            {
               linkTracker = (LinkTracker)enumerator.Value;
               if (linkTracker.IsSender)
               {
                  linkTracker = null;
               }
            }

            return linkTracker;
         }
      }

      public uint FindFreeLocalHandle()
      {
         uint HANDLE_MAX = localBegin?.HandleMax ?? uint.MaxValue;

         for (uint i = 0; i <= HANDLE_MAX; ++i)
         {
            if (!localLinks.Contains(i))
            {
               return i;
            }
         }

         throw new InvalidOperationException("no local handle available for allocation");
      }

      private LinkTracker FindMatchingPendingLinkOpen(Attach remoteAttach)
      {
         foreach (LinkTracker link in senderByNameMap.Values)
         {
            if (link.Name.Equals(remoteAttach.Name) && !link.IsRemotelyAttached && remoteAttach.IsReceiver)
            {
               return link;
            }
         }

         foreach (LinkTracker link in receiverByNameMap.Values)
         {
            if (link.Name.Equals(remoteAttach.Name) && !link.IsRemotelyAttached && remoteAttach.IsSender)
            {
               return link;
            }
         }

         return null;
      }

      #region Handlers for AMQP performatives

      public SessionTracker HandleBegin(Begin remoteBegin, ushort remoteChannel)
      {
         this.remoteBegin = remoteBegin;
         this.remoteChannel = remoteChannel;
         this.nextIncomingId = remoteBegin.NextOutgoingId;

         return this;
      }

      public SessionTracker HandleLocalBegin(Begin localBegin, ushort localChannel)
      {
         this.localBegin = localBegin;
         this.localChannel = localChannel;
         this.nextOutgoingId = localBegin.NextOutgoingId;

         return this;
      }

      public SessionTracker HandleEnd(End end)
      {
         this.remoteEnd = end;
         return this;
      }

      public SessionTracker HandleLocalEnd(End end)
      {
         this.localEnd = end;
         return this;
      }

      public LinkTracker HandleRemoteAttach(Attach attach)
      {
         if (remoteLinks.Contains(attach.Handle))
         {
            throw new AssertionError(string.Format(
                "Received second attach of link handle {0} with name {1}", attach.Handle, attach.Name));
         }

         LinkTracker linkTracker;

         uint? localHandleMax = localBegin == null ? 0 : localBegin.HandleMax == null ? uint.MaxValue : localBegin.HandleMax;

         if (attach.Handle > localHandleMax)
         {
            throw new AssertionError("Session Handle Max [" + localHandleMax + "] Exceeded for link Attach: " + attach.Handle);
         }

         // Check that the remote attach is an original link attach with no corresponding local
         // attach having already been done or not as there should only ever be one instance of
         // a link tracker for any given link.
         linkTracker = FindMatchingPendingLinkOpen(attach);
         if (linkTracker == null)
         {
            if (attach.Role == Role.Sender)
            {
               linkTracker = new ReceiverTracker(this);
               receiverByNameMap.Add(attach.Name, linkTracker);
            }
            else
            {
               linkTracker = new SenderTracker(this);
               senderByNameMap.Add(attach.Name, linkTracker);
            }
         }

         remoteLinks.Add(attach.Handle, linkTracker);
         linkTracker.HandlerRemoteAttach(attach);

         if (linkTracker.RemoteCoordinator != null)
         {
            driver.Sessions.LastOpenedCoordinator = linkTracker;
         }

         return linkTracker;
      }

      public LinkTracker HandleLocalAttach(Attach attach)
      {
         LinkTracker linkTracker = null;
         if (localLinks.Contains(attach.Handle))
         {
            linkTracker = (LinkTracker)localLinks[attach.Handle];
         }

         // Create a tracker for the local side to use to respond to remote
         // performative or to use when invoking local actions but don't validate
         // that it was already sent one as a test might be checking remote handling.
         if (linkTracker == null)
         {
            if (attach.Role == Role.Sender)
            {
               if (!senderByNameMap.TryGetValue(attach.Name, out linkTracker))
               {
                  linkTracker = new SenderTracker(this);
                  senderByNameMap.Add(attach.Name, linkTracker);
               }
            }
            else
            {
               if (!receiverByNameMap.TryGetValue(attach.Name, out linkTracker))
               {
                  linkTracker = new ReceiverTracker(this);
                  receiverByNameMap.Add(attach.Name, linkTracker);
               }
            }

            localLinks.Add(attach.Handle, linkTracker);
            linkTracker.HandleLocalAttach(attach);
         }

         return linkTracker;
      }

      public LinkTracker HandleRemoteDetach(Detach detach)
      {
         LinkTracker tracker;

         if (remoteLinks.Contains(detach.Handle))
         {
            tracker = (LinkTracker)remoteLinks[detach.Handle];
            tracker.HandleRemoteDetach(detach);
            remoteLinks.Remove(detach.Handle);

            if (tracker.IsLocallyDetached)
            {
               if (tracker.IsSender)
               {
                  senderByNameMap.Remove(tracker.Name);
               }
               else
               {
                  receiverByNameMap.Remove(tracker.Name);
               }
            }
         }
         else
         {
            throw new AssertionError(string.Format(
                "Received Detach for unknown remote link with handle {0}", detach.Handle));
         }

         return tracker;
      }

      public LinkTracker HandleLocalDetach(Detach detach)
      {
         LinkTracker tracker = null;

         // Handle the detach and remove if we knew about it, otherwise ignore as
         // the test might be checked for handling of unexpected End frames etc.
         if (localLinks.Contains(detach.Handle))
         {
            tracker = (LinkTracker)localLinks[detach.Handle];
            tracker.HandleLocalDetach(detach);
            localLinks.Remove(detach.Handle);

            if (tracker.IsRemotelyDetached)
            {
               if (tracker.IsSender)
               {
                  senderByNameMap.Remove(tracker.Name);
               }
               else
               {
                  receiverByNameMap.Remove(tracker.Name);
               }
            }
         }

         return tracker;
      }

      public LinkTracker HandleTransfer(Transfer transfer, byte[] payload)
      {
         LinkTracker tracker = null;

         if (remoteLinks.Contains(transfer.Handle))
         {
            tracker = (LinkTracker)remoteLinks[transfer.Handle];
         }

         if (tracker.IsSender)
         {
            throw new AssertionError("Received inbound Transfer addressed to a local Sender link");
         }
         else
         {
            tracker.HandleTransfer(transfer, payload);
            // TODO - Update session state based on transfer
         }

         return tracker;
      }

      public void HandleLocalTransfer(Transfer transfer, byte[] payload)
      {
         LinkTracker tracker = null;

         if (localLinks.Contains(transfer.Handle))
         {
            tracker = (LinkTracker)localLinks[transfer.Handle];
         }

         // Pass along to local sender for processing before sending and ignore if
         // we aren't tracking a link or the link is a receiver as the test might
         // be checking how the remote handles invalid frames.
         if (tracker != null && tracker.IsSender)
         {
            tracker.HandleTransfer(transfer, payload);
            // TODO - Update session state based on transfer
         }
      }

      public void HandleDisposition(Disposition disposition)
      {
         // TODO Forward to attached links or issue errors if invalid.
      }

      public void HandleLocalDisposition(Disposition disposition)
      {
         // TODO Forward to attached links or issue error if invalid.
      }

      public LinkTracker HandleFlow(Flow flow)
      {
         LinkTracker tracker = null;

         if (flow.Handle != null)
         {
            if (remoteLinks.Contains(flow.Handle))
            {
               tracker = (LinkTracker)remoteLinks[flow.Handle];
               tracker.HandleFlow(flow);
            }
            else
            {
               throw new AssertionError(string.Format(
                   "Received Flow for unknown remote link with handle {0}", flow.Handle));
            }
         }

         return tracker;
      }

      #endregion
   }
}