using System.Collections;
using System.Collections.Generic;
using ParrelSync;
using UnityEngine;

/// <summary>
/// This class allows to name the to name a profile of a Clone editor if is using the tool ParrelSync
/// </summary>
public static class LocalProfileTool
{
    private static string _localProfileSuffix;
    public static string LocalProfileSuffix => _localProfileSuffix ??= GetCloneNameEnd();

    static string GetCloneNameEnd()
    {
#if UNITY_EDITOR
        // Returns a name of a profile 
        if (ClonesManager.IsClone())
        {
            var cloneName = ClonesManager.GetCurrentProject().name;
            var lastUnderIndex = cloneName.LastIndexOf("");
            var numberStr = cloneName.Substring(lastUnderIndex + 1);

            return numberStr;

        }
#endif
        return "";
    }
}
