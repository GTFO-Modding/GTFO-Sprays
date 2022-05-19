using CellMenu;
using HarmonyLib;
using Sprays.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Sprays.UI
{
    [HarmonyPatch]
    public class SpraysDropdown
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CM_PlayerLobbyBar._DoPlayIntro_d__110), nameof(CM_PlayerLobbyBar._DoPlayIntro_d__110.MoveNext))]
        public static void Setup(CM_PlayerLobbyBar._DoPlayIntro_d__110 __instance)
        {
            if (IsSetup || __instance.__1__state != -1) return;
            IsSetup = true;

            var clothesButton = CM_PageLoadout.Current.m_playerLobbyBars[0].m_clothesButton;

            var spraysButton = GameObject.Instantiate(clothesButton, CM_PageLoadout.Current.m_staticContentHolder);
            spraysButton.name = "SpraySelectButton";
            spraysButton.gameObject.SetActive(true);

            spraysButton.transform.localPosition = new(-960, 567.5f, 0);
            spraysButton.transform.FindChild("Box/StretchLineT").localScale = new(0.5f, 1, 1);
            spraysButton.transform.FindChild("Box/StretchLineB").localScale = new(0.5f, 1, 1);
            spraysButton.transform.FindChild("Box/StretchLineR").localPosition = new(3.1818f, 27.5f, 0);
            spraysButton.transform.FindChild("Text").localPosition = new(-100, 0, 0);
            spraysButton.transform.FindChild("NewItemIcon").localPosition = new(374.7287f, -134.909f, 0);
            spraysButton.transform.FindChild("NewItemIcon").localScale = new(2,2,2);

            var sprayButtonCollider = spraysButton.GetComponent<BoxCollider2D>();
            sprayButtonCollider.size = new(320, 52.8f);
            sprayButtonCollider.offset = new(-130, 0);

            var text = spraysButton.transform.FindChild("Text").GetComponent<TextMeshPro>();
            text.SetText("Sprays");

            spraysButton.transform.FindChild("NewItemIcon").gameObject.SetActive(false);

            SpraysButton = spraysButton.GetComponent<CM_LobbyScrollItem>();
            SpraysButton.m_parentBar = clothesButton.m_parentBar;
            SpraysButton.m_guiAlign = clothesButton.m_guiAlign;
            SpraysButton.add_OnBtnPressCallback((Action<int>)SpraysButtonCallback);
        }

        public static void SpraysButtonCallback(int x)
        {
            var lobbyBar = SpraysButton.m_parentBar;

            lobbyBar.Select();
            SpraysButton.IsSelected = true;

			lobbyBar.HidePopup();
			lobbyBar.m_popupVisible = true;
			lobbyBar.m_showingClothes = true;
			lobbyBar.m_popupScrollWindow.m_infoBoxWidth = 700f;
			lobbyBar.m_popupScrollWindow.SetSize(new Vector2(1600f, 760f));
			lobbyBar.m_popupScrollWindow.ResetHeaders();
			lobbyBar.m_popupScrollWindow.AddHeader("Sprays", 0);
			lobbyBar.m_popupScrollWindow.SetPosition(new Vector2(0f, 350f));
			lobbyBar.m_popupScrollWindow.RespawnInfoBoxFromPrefab(lobbyBar.m_popupInfoBoxWeaponPrefab);

            CM_BoosterImplantSlotItem sprayCard;
            int i = 0;
            Sprite icon = null;

            Il2CppSystem.Collections.Generic.List<iScrollWindowContent> spraysList = new();
            foreach (var spray in RuntimeLookup.LocalSprays)
            {
                sprayCard = GOUtil.SpawnChildAndGetComp<CM_BoosterImplantSlotItem>(lobbyBar.m_boosterImplantCardPrefab, lobbyBar.transform);
                SprayIcons[i] = sprayCard;

                sprayCard.TextMeshRoot = lobbyBar.m_parentPage.transform;
                sprayCard.SetupFromLobby(lobbyBar.transform, lobbyBar, true);
                sprayCard.ForcePopupLayer(true, null);
                sprayCard.m_icon.sprite = Sprite.Create(spray.Texture, new(0, 0, spray.Texture.width, spray.Texture.height), new(0.5f, 0.5f));
                sprayCard.m_nameText.SetText(spray.m_Name);
                sprayCard.m_subTitleText.SetText(spray.Checksum);
                sprayCard.m_usesText.SetText("");
                sprayCard.ID = i; //Pass the reference to the spray index into the button's ID field
                if (i == SprayInputHandler.Current.m_SprayIndex)
                {
                    icon = sprayCard.m_icon.sprite;
                }

                InfoBoxRef_Temp = lobbyBar.m_popupScrollWindow.InfoBox;
                sprayCard.add_OnBtnPressCallback((Action<int>)((i) => 
                {
                    SprayInputHandler.Current.m_SprayIndex = i;
                    SprayInputHandler.Current.m_ReloadSpray = true;
                    InfoBoxRef_Temp.SetInfoBox("", "", "", "", "", SprayIcons[i].m_icon.sprite);
                }));

                spraysList.Add(sprayCard.TryCast<iScrollWindowContent>());
                i++;
            }
            lobbyBar.m_popupScrollWindow.SetContentItems(spraysList);

            lobbyBar.m_popupScrollWindow.InfoBox.SetInfoBox("", "", "", "", "", icon);
            lobbyBar.m_popupScrollWindow.InfoBox.m_infoMainIcon.size = new(2.56f, 2.56f);
            lobbyBar.m_popupScrollWindow.InfoBox.m_infoMainIcon.sortingOrder = 5;
            lobbyBar.m_popupScrollWindow.InfoBox.m_infoMainIcon.color = Color.white;
            lobbyBar.m_popupScrollWindow.InfoBox.m_infoMainIcon.transform.localPosition = new(-15, 30, -75);
            lobbyBar.m_popupScrollWindow.InfoBox.m_infoMainIcon.transform.localScale = Vector3.one * 200;

            lobbyBar.ShowPopup();
		}

        public static bool IsSetup = false;
        public static CM_LobbyScrollItem SpraysButton;
        public static SpriteRenderer Thumbnail;

        public static CM_BoosterImplantSlotItem[] SprayIcons = new CM_BoosterImplantSlotItem[10];
        public static CM_ScrollWindowInfoBox InfoBoxRef_Temp;
    }
}
