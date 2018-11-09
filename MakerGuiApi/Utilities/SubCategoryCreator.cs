﻿using BepInEx.Logging;
using ChaCustom;
using Harmony;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = BepInEx.Logger;

namespace MakerAPI
{
    internal class SubCategoryCreator
    {
        private static Transform _subCategoryCopy;

        private static Transform SubCategoryCopy
        {
            get
            {
                if (_subCategoryCopy == null)
                {
                    // 00_FaceTop/tglEar is present in Both male and female maker
                    var original = GameObject.Find("00_FaceTop").transform.Find("tglEar");

                    _subCategoryCopy = Object.Instantiate(original, MakerAPI.Instance.transform, true);
                    _subCategoryCopy.gameObject.SetActive(false);

                    var toggle = _subCategoryCopy.GetComponent<Toggle>();
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.isOn = false;

                    _subCategoryCopy.name = "CustomSubcategory (Copy)";
                    var copyTop = _subCategoryCopy.Find("EarTop");
                    copyTop.name = "CustomSubcategoryTop";

                    Object.DestroyImmediate(copyTop.GetComponent<CvsEar>());

                    // This doesn't happen until after we return from the hook, 
                    // so these will still be copied and need to be deleted afterwards
                    foreach (Transform tr in copyTop)
                        Object.Destroy(tr.gameObject);

                    foreach (var renderer in _subCategoryCopy.GetComponentsInChildren<Image>())
                        renderer.raycastTarget = true;
                }
                return _subCategoryCopy;
            }
        }

        public static Transform AddNewSubCategory(UI_ToggleGroupCtrl mainCategory, string subCategoryName)
        {
            Logger.Log(LogLevel.Debug, $"[MakerAPI] Adding custom subcategory {subCategoryName} to {mainCategory.transform.name}");

            var tr = Object.Instantiate(SubCategoryCopy.gameObject, mainCategory.transform, true).transform;
            tr.name = subCategoryName;
            var trTop = tr.Find("CustomSubcategoryTop");
            trTop.name = subCategoryName + "Top";

            foreach (Transform oldObj in trTop.transform)
                Object.Destroy(oldObj.gameObject);

            tr.GetComponentInChildren<TextMeshProUGUI>().text = subCategoryName.StartsWith("tgl") ? subCategoryName.Substring(3) : subCategoryName;

            var tgl = tr.GetComponent<Toggle>();
            tgl.@group = mainCategory.transform.GetComponent<ToggleGroup>();

            var cgroup = trTop.GetComponent<CanvasGroup>();
            mainCategory.items = mainCategory.items.AddToArray(new UI_ToggleGroupCtrl.ItemInfo { tglItem = tgl, cgItem = cgroup });

            foreach (var renderer in _subCategoryCopy.GetComponentsInChildren<UI_RaycastCtrl>())
                renderer.Reset();

            tr.gameObject.SetActive(true);

            return tr;
        }
    }
}