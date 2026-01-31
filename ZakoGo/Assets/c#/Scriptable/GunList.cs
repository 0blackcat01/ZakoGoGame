using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "New GunList", menuName = "Gun/New GunList")]
public class GunList : ScriptableObject
{
    public List<Gun> gunList = new List<Gun>();

}
