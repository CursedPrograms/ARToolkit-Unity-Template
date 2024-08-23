using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

class ARTrackedObjectGizmo
{
    private static Color MarkerBorderSelected = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    private static Color MarkerBorderUnselected = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    private static Color MarkerEdgeSelected = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    private static Color MarkerEdgeUnselected = new Color(0.75f, 0.75f, 0.75f, 0.5f);


	[DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]                                      
    static void RenderARTrackedObjectGizmo(ARTrackedObject to, GizmoType gizmoType)
    {
        DrawMarker(to, (gizmoType & GizmoType.Active) != 0);
    }

    private static void DrawMarker(ARTrackedObject to, bool selected)
	{
        ARMarker m = to.GetMarker();
        if (m == null) return;
		if (!m.gameObject.activeInHierarchy) return;       
		
		if (m.UID == ARMarker.NO_ID) {
			m.Load();
		}

		Matrix4x4 pose = to.gameObject.transform.localToWorldMatrix;
        switch (m.MarkerType) {

            case MarkerType.Square:
            case MarkerType.SquareBarcode:
				DrawSingleMarker(m, pose, selected);
                break;

			case MarkerType.Multimarker:
				DrawMultiMarker(m, pose, selected);
			    break;
			
			case MarkerType.NFT:
				DrawNFTMarker(m, pose, selected);
			    break;

        }
    }

    private static void DrawSingleMarker(ARMarker m, Matrix4x4 mat, bool selected) 
    {
        float pattWidth = m.PatternWidth;
		Vector3 origin = mat.GetColumn(3);
		Vector3 right = mat.GetColumn(0);
		Vector3 up = mat.GetColumn(1);

		DrawRectangle(origin, up, right, pattWidth * 0.5f, pattWidth * 0.5f, selected ? MarkerBorderSelected : MarkerBorderUnselected);   
		DrawRectangle(origin, up, right, pattWidth, pattWidth, selected ? MarkerBorderSelected : MarkerBorderUnselected);  
		DrawRectangle(origin, up, right, pattWidth * 1.05f, pattWidth * 1.05f, selected ? MarkerEdgeSelected : MarkerEdgeUnselected);  

		float wordUnitSize = pattWidth * 0.02f;
		DrawWord(m.Tag, wordUnitSize, origin - up * (pattWidth * 0.6f + (wordUnitSize * 4)) - right * (pattWidth * 0.525f), up, right * 0.5f);
    }

	private static void DrawMultiMarker(ARMarker m, Matrix4x4 mat, bool selected) 
    {
		for (int i = 0; i < m.Patterns.Length; i++) {

			Matrix4x4 mat1 = mat * m.Patterns[i].matrix;

            float pattWidth = m.Patterns[i].width;

			Vector3 origin = mat1.GetColumn(3);
			Vector3 right = mat1.GetColumn(0);
			Vector3 up = mat1.GetColumn(1);

			DrawRectangle(origin, up, right, pattWidth * 0.5f, pattWidth * 0.5f, selected ? MarkerBorderSelected : MarkerBorderUnselected);   
			DrawRectangle(origin, up, right, pattWidth, pattWidth, selected ? MarkerBorderSelected : MarkerBorderUnselected);  
			DrawRectangle(origin, up, right, pattWidth * 1.05f, pattWidth * 1.05f, selected ? MarkerEdgeSelected : MarkerEdgeUnselected);  

			float wordUnitSize = pattWidth * 0.02f;
			DrawWord(m.Tag + "(" + i + ")", wordUnitSize, origin - up * (pattWidth * 0.6f + (wordUnitSize * 4)) - right * (pattWidth * 0.525f), up, right * 0.5f);
		}
               
    }

	private static void DrawNFTMarker(ARMarker m, Matrix4x4 mat, bool selected) 
    {
        float pattWidth = m.NFTWidth;
		float pattHeight = m.NFTHeight;
		if (pattWidth > 0.0f && pattHeight > 0.0f) {

			float biggestSide = Math.Max(pattWidth, pattHeight);
			Vector3 origin = mat.GetColumn(3);
			Vector3 right = mat.GetColumn(0);
			Vector3 up = mat.GetColumn(1);
			Vector3 centre = origin + right*0.5f*pattWidth + up*0.5f*pattHeight;

			DrawRectangle(centre, up, right, pattWidth, pattHeight, selected ? MarkerBorderSelected : MarkerBorderUnselected);
			DrawRectangle(centre, up, right, pattWidth + biggestSide*0.05f, pattHeight + biggestSide*0.05f, selected ? MarkerEdgeSelected : MarkerEdgeUnselected);

			float wordUnitSize = pattHeight * 0.02f;
			DrawWord(m.Tag, wordUnitSize, centre - up*(pattHeight*0.6f + (wordUnitSize*4)) - right*pattWidth*0.525f, up, right*0.5f);
		}
	}
	
	private static void DrawRectangle(Vector3 centre, Vector3 up, Vector3 right, float width, float height, Color color) 
	{

        Gizmos.color = color;

		Vector3 u = up * height;
        Vector3 r = right * width;
        Vector3 p = centre - (u * 0.5f) - (r * 0.5f);
        
        Gizmos.DrawLine(p, p + u);
        Gizmos.DrawLine(p + u, p + u + r);
        Gizmos.DrawLine(p + u + r, p + r);
        Gizmos.DrawLine(p + r, p);
    }
	
	private readonly static Dictionary<char, String> letters = new Dictionary<char, String>() {
		{' ', ""},
		{'!', "RR(U)U(UU)"},
		{'"', "UUUUR(D)URR(D)"},
		{'#', "R(UUUU)RR(DDDD)UR(LLLL)UU(RRRR)"},
		{'$', "RR(UUUU)DR(LL)(D)(RR)(D)(LL)"},
		{'%', "RUUUU(D)DDD(RRUUUU)DDD(D)"},
		{'&', "RRRRUU(DDLL)(L)(UL)(RRRUU)(UL)(DL)(DDDRRR)"},
		{'\'',"RRUUU(U)"},
		{'(', "RRR(UL)(UU)(UR)"},
		{')', "R(UR)(UU)(UL)"},
		{'*', "RR(UUUU)DL(DDRR)UU(DDLL)UL(RRRR)"},
		{'+', "RUU(RR)LU(DD)"},
		{',', "R(UR)"},
		{'-', "UR(RR)"},
		{'.', "RR(U)"},
		{'/', "R(UUUURR)"},		
		{'0', "(UUUU)(RRRR)(DDDD)(LLLL)"},
		{'1', "RR(UUUU)"},
		{'2', "UUU(UR)(RR)(DR)(DDDLLLL)(RRRR)"},
		{'3', "U(DR)(RR)(UR)(ULL)(URR)(UL)(LL)(DL)"},
		{'4', "RRRRU(LLLL)(UUURRR)(DDDD)"},
		{'5', "(RRRR)(UU)(LLLL)(UU)(RRRR)"},
		{'6', "UUUURRR(LL)(DDL)(DD)(RRRR)(UU)(LLLL)"},
		{'7', "UUUU(RRRR)(DDDDLL)"},
		{'8', "(UUUU)(RRRR)(DDDD)(LLLL)UU(RRRR)"},
		{'9', "R(RR)(UUR)(LLLL)(UU)(RRRR)(DD)"},		
		{':', "RR(U)UU(U)"},
		{';', "R(UR)UU(U)"},
		{'<', "RRR(UULL)(RRUU)"},
		{'=', "UR(RR)UU(LL)"},
		{'>', "R(UURR)(LLUU)"},
		{'?', "RR(U)U(R)(UR)(UL)(L)(DL)"},
		{'@', "RRR(LL)(UL)(UU)(UR)(RR)(DR)(DD)(LL)(U)(R)(D)"},		
		{'A', "(UUU)(UR)(RR)(DR)(DDD)UU(LLLL)"},
		{'B', "(UUUU)(RRR)(DR)(DD)(DL)(LLL)UU(RRRR)"},
		{'C', "RRRRU(DL)(LL)(UL)(UU)(UR)(RR)(DR)"},
		{'D', "(UUUU)(RRR)(DR)(DD)(DL)(LLL)"},
		{'E', "(UUUU)(RRRR)DDLL(LL)DD(RRRR)"},
		{'F', "(UUUU)(RRRR)DDLL(LL)"},
		{'G', "UURR(RR)(DD)(LLL)(UL)(UU)(UR)(RR)(DR)"},
		{'H', "(UUUU)DD(RRRR)UU(DDDD)"},
		{'I', "(RRRR)LL(UUUU)LL(RRRR)"},
		{'J', "U(D)(RR)(UUUU)LL(RRRR)"},
		{'K', "(UUUU)RRRR(DDLLLL)(DDRRRR)"},
		{'L', "UUUU(DDDD)(RRRR)"},
		{'M', "(UUUU)(DDRR)(UURR)(DDDD)"},
		{'N', "(UUUU)(DDDDRRRR)(UUUU)"},
		{'O', "U(UU)(UR)(RR)(DR)(DD)(DL)(LL)(UL)"},
		{'P', "(UUUU)(RRR)(DR)(D)(DL)(LLL)"},
		{'Q', "U(UU)(UR)(RR)(DR)(DD)(DL)(LL)(UL)RRR(DR)"},
		{'R', "(UUUU)(RRR)(DR)(D)(DL)(LLL)RRR(DR)"},
		{'S', "U(DR)(RR)(UR)(UL)(LL)(UL)(UR)(RR)(DR)"},
		{'T', "RR(UUUU)LL(RRRR)"},
		{'U', "UUUU(DDDD)(RRR)(UR)(UUU)"},
		{'V', "UUUU(DDDDRR)(UUUURR)"},
		{'W', "UUUU(DDDDR)(UUR)(DDR)(UUUUR)"},
		{'X', "(UUUURRRR)LLLL(DDDDRRRR)"},
		{'Y', "UUUU(DDRR)(UURR)DDLL(DD)"},
		{'Z', "UUUU(RRRR)(DDDDLLLL)(RRRR)"},
		{'[', "RRR(L)(UUUU)(R)"},
		{'\\',"RRR(UUUULL)"},
		{']', "R(R)(UUUU)(L)"},
		{'^', "UUR(UUR)(DDR)"},
		{'_', "(RRRR)"},
		{'`', "UUUURR(DR)"},
		{'{', "RRR(L)(U)(LU)(RU)(U)(R)"},
		{'|', "RR(UUUU)"},
		{'}', "R(R)(U)(RU)(LU)(U)(L)"},
		{'~', "UU(UR)(DDRR)(UR)"}		
	};
	
	private static void DrawWord(String word, float size, Vector3 origin, Vector3 forward, Vector3 right) 
	{
		foreach (char c in word.ToUpper()) {
			DrawLetter(c, size, origin, forward, right);
			origin += right * size * 6.0f;
		}
	}
	
	private static void DrawLetter(char letter, float size, Vector3 origin, Vector3 forward, Vector3 right) 
	{
		String path = letters[letter];
		
		Vector3 f = forward * size;
		Vector3 r = right * size;
		
		Vector3 down = origin;
		Vector3 current = origin;
		
		foreach (char c in path)  {
			switch (c) {
                case '(':
                    down = current;
                    continue;
                case ')':
                    Gizmos.DrawLine(down, current);
                    continue;
                case 'U':
                    current += f;
                    break;
                case 'D':
                    current -= f;
                    break;
                case 'R':
                    current += r;
                    break;
                case 'L':
                    current -= r;
                    break;			
			}
		}
	}

}




