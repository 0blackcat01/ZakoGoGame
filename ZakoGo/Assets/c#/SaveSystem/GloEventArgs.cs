using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward_Args : EventArgs
{
    public int reward_num;

    
}
public class Sex_Args : EventArgs
{
    public int ID=0;
}
public class Item_Args : EventArgs
{
    public Item itemarg;
    public int AddmoneyNum;

}