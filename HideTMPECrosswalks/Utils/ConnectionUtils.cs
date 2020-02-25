using GenericGameBridge.Service;
using System.Collections.Generic;
using TrafficManager.Manager.Impl;
using TrafficManager.State;

namespace HideTMPECrosswalks.Utils
{
    public static class ConnectionUtils
    {
        static LaneConnectionManager LCMan => LaneConnectionManager.Instance;
        static  INetService netService => TrafficManager.Constants.ServiceFactory.NetService;
        public static NetLane[] laneBuffer => NetManager.instance.m_lanes.m_buffer;
        public static ref NetLane ToLane(this uint laneId) => ref laneBuffer[laneId];

#if false
        const NetInfo.LaneType LANE_TYPES = NetInfo.LaneType.PublicTransport;

        // use VehicleInfo.VehicleType.Tram/Train/Monorail
        public static bool AreTracksConnected(
            ushort nodeID, 
            ushort segmentID1, 
            ushort segmentID2, 
            VehicleInfo.VehicleType vehicleType)
        {
            bool startNode1 = (bool)netService.IsStartNode(segmentID1, nodeID);
            bool startNode2 = (bool)netService.IsStartNode(segmentID2, nodeID);
            var lanes1 = netService.GetSortedLanes(
                segmentID1,
                ref segmentID1.ToSegment(),
                startNode1,
                LANE_TYPES,
                vehicleType);
            var lanes2 = netService.GetSortedLanes(
                segmentID1,
                ref segmentID2.ToSegment(),
                startNode2,
                LANE_TYPES,
                vehicleType);

            foreach (var lane1 in lanes1)
            {
                bool b1 = LCMan.HasConnections(lane1.laneId, startNode1);
                if (!b1) {
                    return true;
                }
                foreach (var lane2 in lanes2) {
                    bool b2 = LCMan.AreLanesConnected(lane1.laneId, lane2.laneId, startNode1);
                    if (b2) { 
                        return true; 
                    }
                }
            }
            return false;
        }

        public static bool ShouldConnectTracks(uint sourceLaneID, uint targetLaneID, bool sourceStartNode)
        {
            bool b1 = LCMan.HasConnections(sourceLaneID, sourceStartNode);
            bool b2 = LCMan.AreLanesConnected(sourceLaneID, targetLaneID, sourceStartNode);
            return !b1 | b2;
        }

        public static bool ShouldConnectTracks(
            int sourceLaneIndex,
            ushort sourceSegmentID,
            int targetLaneIndex, 
            ushort targetSegmentID,
            ushort nodeID)
        {
            bool sourceStartNode = (bool)netService.IsStartNode(sourceSegmentID, nodeID);
            uint sourceLaneID = GetLaneID(sourceSegmentID, (byte)sourceLaneIndex);
            uint targetLaneID = GetLaneID(targetSegmentID, (byte)targetLaneIndex);
            return ShouldConnectTracks(sourceLaneID, targetLaneID, sourceStartNode);
        }



        public static uint GetLaneID(ushort segmentID, byte laneIndex)
        {
            var m_lanes = segmentID.ToSegment().Info.m_lanes;
            int n = m_lanes.Length;
            uint laneID = segmentID.ToSegment().m_lanes;
            for(byte idx = 0; idx < n; idx++)
            {
                if(idx == laneIndex) {
                    return laneID;
                }
                laneID = laneBuffer[laneID].m_nextLane;
            }
            return 0;
        }

        public static IEnumerable<uint> segmentLanes(
            ushort segmentId,
            NetInfo.LaneType laneType,
            VehicleInfo.VehicleType vehicleType)
        {
            var laneInfos = segmentId.ToSegment().Info.m_lanes;
            int n = laneInfos.Length;

            uint laneId;
            int laneIndex;
            for (laneIndex = 0, laneId = segmentId.ToSegment().m_lanes;
                laneIndex < n;
                ++laneIndex, laneId = laneBuffer[laneId].m_nextLane)
            {
                if ((laneInfos[laneIndex].m_laneType & laneType) == 0 ||
                    (laneInfos[laneIndex].m_vehicleType & vehicleType) == 0)
                {
                    continue;
                }
                yield return laneId;
            }
        }
#endif
        // assuming that the segments can have connected lanes.
        public static bool ShouldConnectTracks(
            ushort sourceSegmentId,
            ushort targetSegmentId,
            ushort nodeId,
            NetInfo.LaneType laneType,
            VehicleInfo.VehicleType vehicleType)
        {
            bool sourceStartNode = (bool)netService.IsStartNode(sourceSegmentId, nodeId);
            var sourceLaneInfos = sourceSegmentId.ToSegment().Info.m_lanes;
            int nSource = sourceLaneInfos.Length;
            
            var targetLaneInfos = targetSegmentId.ToSegment().Info.m_lanes;
            int nTarget = targetLaneInfos.Length;
            
            uint sourceLaneId, targetLaneId;
            int sourceLaneIndex, targetLaneIndex;
            for (sourceLaneIndex = 0, sourceLaneId = sourceSegmentId.ToSegment().m_lanes; 
                sourceLaneIndex < nSource; 
                ++sourceLaneIndex, sourceLaneId = laneBuffer[sourceLaneId].m_nextLane)
            {
                if ((sourceLaneInfos[sourceLaneIndex].m_laneType & laneType) == 0 ||
                    (sourceLaneInfos[sourceLaneIndex].m_vehicleType & vehicleType) == 0)
                {
                    continue;
                }
                for (targetLaneIndex = 0, targetLaneId = targetSegmentId.ToSegment().m_lanes; 
                    targetLaneId < nTarget; 
                    ++targetLaneIndex, targetLaneId = laneBuffer[targetLaneId].m_nextLane)
                {
                    if ((targetLaneInfos[targetLaneIndex].m_laneType & laneType) == 0 ||
                        (targetLaneInfos[targetLaneIndex].m_vehicleType & vehicleType) == 0)
                    {
                        continue;
                    }
                    bool b1 = LCMan.HasConnections(sourceLaneId, sourceStartNode);
                    bool b2 = LCMan.AreLanesConnected(sourceLaneId, targetLaneId, sourceStartNode);
                    if (!b1 || b2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


    }
}
