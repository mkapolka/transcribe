using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Teaching {
    public Unit.Type unitType;
    public Town destinationTown;
    public Heresy heresy;

    public enum TravelType {
        Specific, Pilgrimage
    }

    public struct TravelInfo {
        TravelType type;
        Town town;
    }

    public enum Heresy {
        Rich, Poor
    }

    public Town[] GetPatrolRoute() {
        return new Town[0];
    }
}
