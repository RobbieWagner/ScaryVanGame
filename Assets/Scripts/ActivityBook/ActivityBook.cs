using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RobbieWagnerGames.ScaryVanGame
{
    public class ActivityBook : MonoBehaviour
    {
        [Header("Pages")]
        [SerializeField] private List<ActivityBookPage> pagePrefabs;
        private List<ActivityBookPage> pageInstances = new List<ActivityBookPage>();
        private int currentPageIndex = 0;
        public int CurrentPageIndex
        {
            get => currentPageIndex;
            set
            {
                if (value < 0 || value == currentPageIndex || value > pagePrefabs.Count / 2 || pageTurnCo != null || !canFlipPages)
                    return;

                pageTurnCo = StartCoroutine(TurnPage(currentPageIndex, value));
				currentPageIndex = value;
            }
        }

        [SerializeField] private Canvas leftPageCanvas;
        [SerializeField] private Canvas rightPageCanvas;

        [Header("Manager UI")]
        [SerializeField] private Canvas bookMarginCanvas;
        [SerializeField] private Button prevPageButton;
        [SerializeField] private Button nextPageButton;

        [Header("Animation")]
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
            prevPageButton.onClick.AddListener(TurnPreviousPage);
            nextPageButton.onClick.AddListener(TurnNextPage);

            InitializeActivityBook();

            StartCoroutine(OpenBookCo(new int[] {0,1}, ()=>{canFlipPages = true;}));
        }

        private void InitializeActivityBook()
        {
            // Instantiate page prefabs on the correct page
            for(int i = 0; i < pagePrefabs.Count; i++)
            {
                bool placeOnLeft = i % 2 == 0;
                Transform parent = placeOnLeft ? leftPageCanvas.transform : rightPageCanvas.transform;

                pageInstances.Add(Instantiate(pagePrefabs[i], parent));
                pageInstances[i].gameObject.SetActive(false);
            }
        }

        private void TurnPreviousPage()
        {
            CurrentPageIndex--;
        }

        private void TurnNextPage()
        {
            CurrentPageIndex++;
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

        private IEnumerator CloseBookCo(bool closeFromBack, int[] pagesToClose)
        {
            Debug.Log("closing book");
            closeCompleted = false;

            animator.SetTrigger(closeFromBack ? CLOSE_BACK_TRIGGER : CLOSE_FRONT_TRIGGER);

            yield return new WaitUntil(() => closeCompleted);

            ActivityBookPage leftPage = pageInstances[pagesToClose[0]];
            ActivityBookPage rightPage = null;
            if (pageInstances.Count > pagesToClose[1])
                rightPage = pageInstances[pagesToClose[1]];

            Debug.Log(leftPage.gameObject.name);
            leftPage.gameObject.SetActive(false);
            rightPage?.gameObject.SetActive(false);
        }

        private IEnumerator OpenBookCo(int[] pagesToOpen, Action callback = null)
        {
            Debug.Log("opening book");
            ActivityBookPage leftPage = pageInstances[pagesToOpen[0]];
            ActivityBookPage rightPage = null;
            if (pageInstances.Count > pagesToOpen[1])
                rightPage = pageInstances[pagesToOpen[1]];
            
            leftPage.gameObject.SetActive(true);
            rightPage?.gameObject.SetActive(true);

            openCompleted = false;
            animator.SetTrigger(OPEN_TRIGGER);
            yield return new WaitUntil(() => openCompleted);

            callback?.Invoke();
        }

        private IEnumerator TurnPage(int indexFrom, int indexTo)
        {
            int[] pagesToClose = {indexFrom * 2, indexFrom * 2 + 1};
            int[] pagesToOpen = {indexTo * 2, indexTo * 2 + 1};

            Debug.Log("turn page triggered");
            if (indexFrom >= 0)
            {
                bool closeFromBack = indexFrom > indexTo ? true : false;
                yield return CloseBookCo(closeFromBack, pagesToClose);
            }

            yield return OpenBookCo(pagesToOpen);

            pageTurnCo = null;
        }
    }
}