using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RobbieWagnerGames.ScaryVanGame
{
    public class ActivityBook : MonoBehaviour
    {
        [SerializeField] private List<ActivityBookPage> pagePrefabs;
        private int currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => currentPageIndex;
            set
            {
                if (value < 0 || value == currentPageIndex || value > pagePrefabs.Count / 2 || pageTurnCo != null || !canFlipPages)
                    return;

				currentPageIndex = value;
                pageTurnCo = StartCoroutine(TurnPage(currentPageIndex, value));
            }
        }

        [SerializeField] private Canvas leftPageCanvas;
        [SerializeField] private Canvas rightPageCanvas;

        private ActivityBookPage leftPageContent = null;
        private ActivityBookPage rightPageContent = null;

        [SerializeField] private Animator animator;

        #region animator parameters
        private const string OPEN_TRIGGER = "Open";
        private const string CLOSE_FRONT_TRIGGER = "CloseFront";
        private const string CLOSE_BACK_TRIGGER = "CloseBack";
        #endregion

        public Action OnOpenBook = null;
        public Action OnCloseBook = null;

        private bool closeCompleted = false;
        private bool openCompleted = false;
        private Coroutine pageTurnCo = null;

        private bool canFlipPages = false;

        private void Awake()
        {
            StartCoroutine(OpenBookCo(new int[] {0,1}, ()=>{canFlipPages = true;}));
        }

        public void OnOpen()
        {
            Debug.Log("on open triggered");
            openCompleted = true;
            OnOpenBook?.Invoke();
        }

        public void OnClosed()
        {
            Debug.Log("on closed triggered");
            closeCompleted = true;
            OnCloseBook?.Invoke();
        }

        private IEnumerator CloseBookCo(bool closeFromBack)
        {
            Debug.Log("closing book");
            closeCompleted = false;

            animator.SetTrigger(closeFromBack ? CLOSE_BACK_TRIGGER : CLOSE_FRONT_TRIGGER);

            yield return new WaitUntil(() => closeCompleted);

            if (rightPageContent != null)
            {
                Destroy(rightPageContent.gameObject);
                rightPageContent = null;
            }
            if (leftPageContent != null)
            {    
                Destroy(leftPageContent.gameObject);
                leftPageContent = null;
            }
        }

        private IEnumerator OpenBookCo(int[] pagesToOpen, Action callback = null)
        {
            Debug.Log("opening book");
            leftPageContent = Instantiate(pagePrefabs[pagesToOpen[0]], leftPageCanvas.transform);
            if (pagesToOpen.Count() > 1)
                rightPageContent = Instantiate(pagePrefabs[pagesToOpen[1]], rightPageCanvas.transform);

            openCompleted = false;
            animator.SetTrigger(OPEN_TRIGGER);
            yield return new WaitUntil(() => openCompleted);

            callback?.Invoke();
        }

        private IEnumerator TurnPage(int pageFrom, int pageTo)
        {
            Debug.Log("turn page triggered");
            if (pageFrom >= 0)
            {
                bool closeFromBack = pageFrom > pageTo ? true : false;
                yield return CloseBookCo(closeFromBack);
            }

            int[] pagesToOpen = {pageTo * 2, pageTo * 2 + 1};
            yield return OpenBookCo(pagesToOpen);

            pageTurnCo = null;
        }
    }
}