using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position
{
    string name, abbreviation, position;

    public Position(string nm, string abb, string pos)
    {
        name = nm;
        abbreviation = abb;
        position = pos;
    }

    public string GetAbbreviation()
    {
        return abbreviation;
    }

    public static Position FromVector2(Vector2 v)
    {
        if (v == Vector2.zero) return Position.unknown;
        if (v.y < RankingGlobal.Constants.PENALTY_AREA_START) return Position.goalkeeper;
        else if (v.y < RankingGlobal.Constants.MAX_OFFENSIVENESS_DEFENDER) return Position.DefenderSelection(v);
        else if (v.y < RankingGlobal.Constants.MAX_OFFENSIVENESS_DEF_MID) return Position.defensiveMidfielder;
        else if (v.y < RankingGlobal.Constants.MAX_OFFENSIVENESS_MIDFIELDER) return Position.MidfielderSelection(v);
        else if (v.y < RankingGlobal.Constants.MAX_OFFENSIVENESS_OFF_MID) return Position.attackingMidfielder;
        else return Position.StrikerSelection(v);
    }

    public bool Equals(Position other)
    {
        return position == other.position;
    }

    public override string ToString()
    {
        return abbreviation;
    }

    public bool IsDefensive()
    {
        return position == "T" || position == "D";
    }

    public bool IsMidfielder()
    {
        return position == "M";
    }

    public bool IsOffensive()
    {
        return position == "S";
    }

    public static Position DefenderSelection(Vector2 pos)
    {
        if (pos.x < 0.33f) return Position.leftDefender;
        if (pos.x > 0.66f) return Position.rightDefender;
        return Position.defender;
    }   
    
    public static Position MidfielderSelection(Vector2 pos)
    {
        if (pos.x < 0.33f) return Position.leftMidfielder;
        if (pos.x > 0.66f) return Position.rightMidfielder;
        return Position.midfielder;
    }  
    
    public static Position StrikerSelection(Vector2 pos)
    {
        if (pos.x < 0.33f) return Position.leftStriker;
        if (pos.x > 0.66f) return Position.rightStriker;
        return Position.striker;
    }

    public static Position unknown = new Position("Unbekannt", "?", "?");
    public static Position leftStriker = new Position("Rechstaußen", "RA", "S");
    public static Position striker = new Position("Stürmer", "ST", "S");
    public static Position rightStriker = new Position("Linksaußen", "LA", "S");
    public static Position attackingMidfielder = new Position("Offensiver Mittelfeldspieler", "OM", "M");
    public static Position leftMidfielder = new Position("Linker Mittelfeldspieler", "LM", "M");
    public static Position midfielder = new Position("Mittelfeldspieler", "MF", "M");
    public static Position rightMidfielder = new Position("Rechter Mittelfeldspieler", "RM", "M");
    public static Position defensiveMidfielder = new Position("Defensiver Mittelfeldspieler", "DM", "M");
    public static Position leftDefender = new Position("Linksverteidiger", "LV", "V");
    public static Position defender = new Position("Verteidiger", "VT", "V");
    public static Position rightDefender = new Position("Rechtsverteidiger", "RV", "V");
    public static Position goalkeeper = new Position("Torhüter", "TW", "T");
}
