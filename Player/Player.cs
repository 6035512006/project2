using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : SingletonMonobehaviour<Player>
{
    // public GameObject canyonOakTreePrefab;
    private WaitForSeconds afterLiftToolAnimationPause;
    private WaitForSeconds afterUseToolAnimationPause;
    private WaitForSeconds afterPickAnimationPause;
    private AnimationOverrides animationOverrides; 
    private GridCursor gridCursor;
    private Cursor cursor;

    private float xInput;
    private float yInput;
    private bool isCarrying = false;
    private bool isIdle;
    private bool isLiftingToolRight;
    private bool isLiftingToolLeft;
    private bool isLiftingToolUp;
    private bool isLiftingToolDown;
    private bool isRunning;
    private bool isUsingToolRight;
    private bool isUsingToolLeft;
    private bool isUsingToolUp;
    private bool isUsingToolDown;
    private bool isSwingingToolRight;
    private bool isSwingingToolLeft; 
    private bool isSwingingToolUp;
    private bool isSwingingToolDown;
    private bool isWalking;
    private bool isPickingRight;
    private bool isPickingLeft;
    private bool isPickingUp;
    private bool isPickingDown;
    private WaitForSeconds liftToolAnimationPause;
    private WaitForSeconds pickAnimationPase;


    private Camera mainCamera;
    private bool playerToolUseDisabled = false;





    private ToolEffect toolEffect = ToolEffect.none;

    private Rigidbody2D rigidBody2D;
    private WaitForSeconds useToolAnimationPause;

#pragma warning disable 414
    private Direction playerDirection;
 #pragma warning restore 414   

    private List<CharacterAttribute> characterAttributeCustomisationList;
    private float movementSpeed;

    [Tooltip("Should be populated in the prefab with the equipped item sprite renderer")]
    [SerializeField] private SpriteRenderer equippedItemSpriteRenderer = null;


    private CharacterAttribute armsCharacterAttribute;
    private CharacterAttribute toolCharacterAttribute;


    private bool _playerInputIsDisabled = false;
    
    public bool PlayerInputIsDisabled { get => _playerInputIsDisabled; set => _playerInputIsDisabled = value; }

    protected override void Awake()
    {
        base.Awake();

        rigidBody2D = GetComponent<Rigidbody2D>();

        animationOverrides = GetComponentInChildren<AnimationOverrides>();

        armsCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.arms,PartVariantColour.none,PartVariantType.none);
        toolCharacterAttribute = new CharacterAttribute(CharacterPartAnimator.tool,PartVariantColour.none,PartVariantType.none);




        characterAttributeCustomisationList = new List<CharacterAttribute>();

        //get reference to main camera
        mainCamera = Camera.main;
    }


    private void OnDisable()
    {

        EventHandler.BeforeSceneUnloadFadeOutEvent -= DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent -= EnablePlayerInput;


    }

    private void OnEnable()
    {
        EventHandler.BeforeSceneUnloadFadeOutEvent += DisablePlayerInputAndResetMovement;
        EventHandler.AfterSceneLoadFadeInEvent += EnablePlayerInput;
    }


    private void Start()
    {
        gridCursor = FindObjectOfType<GridCursor>();
        cursor = FindObjectOfType<Cursor>();
        useToolAnimationPause = new WaitForSeconds(Settings.useToolAnimationPause);
        liftToolAnimationPause = new WaitForSeconds(Settings.liftToolAnimationPause);
        pickAnimationPase = new WaitForSeconds(Settings.pickAnimationPause);
        afterUseToolAnimationPause = new WaitForSeconds(Settings.afterUseToolAnimationPause);
        afterLiftToolAnimationPause = new WaitForSeconds(Settings.afterLiftToolAnimationPause);
        afterPickAnimationPause = new WaitForSeconds(Settings.afterPickAnimationPause);
    }

    private void Update()
    {
        #region Player Input

        if(!PlayerInputIsDisabled)
        {
            ResetAnimationTriggers();

            PlayerMovementInput();

            PlayerWalkInput();

            PlayerClickInput();

            PlayerTestInput();


            


            EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
            false, false, false, false);

        }

        #endregion
    }

    private void FixedUpdate()
    {
        PlayerMovement();
    }

    private void PlayerMovement()
    {
        Vector2 move = new Vector2(xInput * movementSpeed * Time.deltaTime, yInput * movementSpeed * Time.deltaTime);

        rigidBody2D.MovePosition(rigidBody2D.position + move);
    }
    


    private void ResetAnimationTriggers()
    {

        isPickingRight = false;
        isPickingLeft = false;
        isPickingUp = false;
        isPickingDown = false;

        isUsingToolRight = false;
        isUsingToolLeft= false;
        isUsingToolUp = false;
        isUsingToolDown = false;
        isLiftingToolRight = false;
        isLiftingToolLeft = false;
        isLiftingToolUp = false;
        isLiftingToolDown = false;

        isSwingingToolRight = false;
        isSwingingToolLeft = false;
        isSwingingToolUp = false;
        isSwingingToolDown = false;

        toolEffect=ToolEffect.none;
    }

    private void PlayerMovementInput()
    {
        yInput = Input.GetAxisRaw("Vertical");
        xInput = Input.GetAxisRaw("Horizontal");

        if(yInput != 0 && xInput != 0 )
        {
            xInput = xInput * 0.71f;
            yInput = yInput * 0.71f;
        }

        if(xInput != 0 || yInput != 0 )
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;

            if(xInput < 0)
            {
                playerDirection = Direction.left;
            }
            else if(xInput > 0)
            {
                playerDirection = Direction.right;
            }
            else if(yInput < 0)
            {
                playerDirection = Direction.down;
            }
            else 
            {
                playerDirection = Direction.up;
            }
        }
        else if(xInput == 0 && yInput == 0 )
        {
            isRunning = false;
            isWalking = false;
            isIdle = true;
        }
    }

    private void PlayerWalkInput()
    {
        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            isRunning = false;
            isWalking = true;
            isIdle = false;
            movementSpeed = Settings.walkingSpeed;
        }
        else
        {
            isRunning = true;
            isWalking = false;
            isIdle = false;
            movementSpeed = Settings.runningSpeed;
        }
    }


    private void PlayerClickInput()
    {

        if(!playerToolUseDisabled)
        {
            if(Input.GetMouseButton(0))
            {

                if(gridCursor.CursorIsEnable || cursor.CursorIsEnable)
                {
                    Vector3Int cursorGridPosition = gridCursor.GetGridPositionForCursor();
                    Vector3Int playerGridPosition = gridCursor.GetGridPositionForPlayer();
                    ProcessPlayerClickInput(cursorGridPosition,playerGridPosition);
                }
            }
        }
    }


    private void ProcessPlayerClickInput(Vector3Int cursorGridPosition,Vector3Int playerGridPosition)
    {
        ResetMovement();


        Vector3Int playerDirection = GetPlayerClickDirection(cursorGridPosition,playerGridPosition);

        GridPropertyDetails gridPropertyDetails = GridPropertiesManager.Instance.GetGridPropertyDetails(cursorGridPosition.x,cursorGridPosition.y);

        ItemDetails itemDetails = InventoryManager.Instance.GetSelectedInventoryItemDetails(InventoryLocation.player);

        if(itemDetails != null)
        {
            switch(itemDetails.itemType)
            {
                case ItemType.Seed:
                    if(Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputSeed(gridPropertyDetails,itemDetails);
                    }
                    break;

                case ItemType.Commodity:
                    if(Input.GetMouseButtonDown(0))
                    {
                        ProcessPlayerClickInputCommodity(itemDetails);
                    }
                    break;
                case ItemType.Watering_tool:
                case ItemType.Hoeing_tool:
                case ItemType.Reaping_tool:
                case ItemType.Collecting_tool:
                    ProcessPlayerClickInputTool(gridPropertyDetails,itemDetails,playerDirection);
                    break;

                case ItemType.none:
                    break;
                
                case ItemType.count:
                    break;

                default:
                    break;
            }
        }
    }

    private Vector3Int GetPlayerClickDirection(Vector3Int cursorGridPosition,Vector3Int playerGridPosition)
    {
        if(cursorGridPosition.x > playerGridPosition.x)
        {
            return Vector3Int.right;
        }
        else if(cursorGridPosition.x < playerGridPosition.x)
        {
            return Vector3Int.left;
        }
        else if(cursorGridPosition.y > playerGridPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }

    private Vector3Int GetPlayerDirection(Vector3 cursorPosition,Vector3 playerPosition)
    {
        if(
            cursorPosition.x > playerPosition.x 
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.right;
        }
        else if(
            cursorPosition.x < playerPosition.x 
            &&
            cursorPosition.y < (playerPosition.y + cursor.ItemUseRadius / 2f)
            &&
            cursorPosition.y > (playerPosition.y - cursor.ItemUseRadius / 2f)
            )
        {
            return Vector3Int.left;
        }
        else if(cursorPosition.y > playerPosition.y)
        {
            return Vector3Int.up;
        }
        else
        {
            return Vector3Int.down;
        }
    }




    private void ProcessPlayerClickInputSeed(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails)
    {
        if(itemDetails.canBeDropped && gridCursor.CursorPositionIsValid && gridPropertyDetails.daysSinceDug > -1 && gridPropertyDetails.seedItemCode == -1)
        {
            PlantSeedAtCursor(gridPropertyDetails,itemDetails);
        }
        else if(itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
        {
            EventHandler.CallDropSelectedItemEvent();
        }
    }


    private void PlantSeedAtCursor(GridPropertyDetails gridPropertyDetails,ItemDetails itemDetails)
    {
        gridPropertyDetails.seedItemCode = itemDetails.itemCode;
        gridPropertyDetails.growthDays = 0;


        GridPropertiesManager.Instance.DisplayPlantedCrop(gridPropertyDetails);

        EventHandler.CallRemoveSelectedItemFromInventoryEvent();

    }






    private void ProcessPlayerClickInputCommodity(ItemDetails itemDetails)
    {
         if(itemDetails.canBeDropped && gridCursor.CursorPositionIsValid)
         {
             EventHandler.CallDropSelectedItemEvent();
         }
    }


    private void ProcessPlayerClickInputTool(GridPropertyDetails gridPropertyDetails, ItemDetails itemDetails,Vector3Int playerDirection)
    {

        switch(itemDetails.itemType)
        {

            case ItemType.Hoeing_tool:
                if(gridCursor.CursorPositionIsValid)
                {
                    HoeGroundAtCursor(gridPropertyDetails,playerDirection);
                }
                break;
            
            case ItemType.Watering_tool:
                if(gridCursor.CursorPositionIsValid)
                {
                    WaterGroundAtCursor(gridPropertyDetails, playerDirection);
                }
                break;

            case ItemType.Collecting_tool:
                if(gridCursor.CursorPositionIsValid)
                {
                    CollectInPlayerDirection(gridPropertyDetails, itemDetails,playerDirection);
                }
                break;

            case ItemType.Reaping_tool:
                if(cursor.CursorPositionIsValid)
                {
                    playerDirection = GetPlayerDirection(cursor.GetWorldPositionForCursor(),GetPlayerCentrePosition());
                    ReapInPlayerDirectionAtCursor(itemDetails,playerDirection);
                }
                break;
            
            default:
                break;
        }
    }

    private void HoeGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {

        StartCoroutine(HoeGroundAtCursorRoutine(playerDirection,gridPropertyDetails));
    }

    private IEnumerator HoeGroundAtCursorRoutine(Vector3Int playerDirection,GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;
    

        toolCharacterAttribute.partVariantType = PartVariantType.hoe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        if(playerDirection == Vector3Int.right)
        {
            isUsingToolRight = true;
        }
        else if(playerDirection == Vector3Int.left)
        {
            isUsingToolLeft = true;
        }
        else if(playerDirection == Vector3Int.up)
        {
            isUsingToolUp = true;
        }
        else if(playerDirection == Vector3Int.down)
        {
            isUsingToolDown = true;
        }

        yield return useToolAnimationPause;

        if(gridPropertyDetails.daysSinceDug == -1)
        {
            gridPropertyDetails.daysSinceDug = 0;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);


        GridPropertiesManager.Instance.DisplayDugGround(gridPropertyDetails);




        yield return afterUseToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }

    private void WaterGroundAtCursor(GridPropertyDetails gridPropertyDetails, Vector3Int playerDirection)
    {
        StartCoroutine(WaterGroundAtCursorRoutine(playerDirection,gridPropertyDetails));
    }


    private IEnumerator WaterGroundAtCursorRoutine(Vector3Int playerDirection,GridPropertyDetails gridPropertyDetails)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;
    

        toolCharacterAttribute.partVariantType = PartVariantType.wateringCan;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        toolEffect = ToolEffect.watering;

        if(playerDirection == Vector3Int.right)
        {
            isLiftingToolRight = true;
        }
        else if(playerDirection == Vector3Int.left)
        {
            isLiftingToolLeft = true;
        }
        else if(playerDirection == Vector3Int.up)
        {
            isLiftingToolUp = true;
        }
        else if(playerDirection == Vector3Int.down)
        {
            isLiftingToolDown = true;
        }

        yield return liftToolAnimationPause;

        if(gridPropertyDetails.daysSinceWatered == -1)
        {
            gridPropertyDetails.daysSinceWatered= 0;
        }

        GridPropertiesManager.Instance.SetGridPropertyDetails(gridPropertyDetails.gridX, gridPropertyDetails.gridY, gridPropertyDetails);


        GridPropertiesManager.Instance.DisplayWateredGround(gridPropertyDetails);



        yield return afterLiftToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }


    private void CollectInPlayerDirection(GridPropertyDetails gridPropertyDetails, ItemDetails equippedItemDetails,Vector3Int playerDirection)
    {
        StartCoroutine(CollectInPlayerDirectionRoutine(gridPropertyDetails,equippedItemDetails,playerDirection));
    }

    private IEnumerator CollectInPlayerDirectionRoutine(GridPropertyDetails gridPropertyDetails,ItemDetails equippedItemDetails,Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;
    
        ProcessCropWithEquippedItemInPlayerDirection(playerDirection,equippedItemDetails,gridPropertyDetails);

        yield return pickAnimationPase;


        yield return afterPickAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }







    private void ReapInPlayerDirectionAtCursor(ItemDetails itemDetails,Vector3Int playerDirection)
    {
        StartCoroutine(ReapInPlayerDirectionAtCursorRoutine(itemDetails,playerDirection));
    }


    private IEnumerator ReapInPlayerDirectionAtCursorRoutine(ItemDetails itemDetails, Vector3Int playerDirection)
    {
        PlayerInputIsDisabled = true;
        playerToolUseDisabled = true;
    

        toolCharacterAttribute.partVariantType = PartVariantType.scythe;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(toolCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        UseToolInPlayerDirection(itemDetails,playerDirection);

        


        yield return useToolAnimationPause;

        PlayerInputIsDisabled = false;
        playerToolUseDisabled = false;

    }

    private void UseToolInPlayerDirection(ItemDetails equippedItemDetails,Vector3Int playerDirection)
    {
        if(Input.GetMouseButton(0))
        {
            switch(equippedItemDetails.itemType)
            {
                case ItemType.Reaping_tool:
                    if(playerDirection == Vector3Int.right)
                    {
                        isSwingingToolRight =true;
                    }
                    else if(playerDirection == Vector3Int.left)
                    {
                        isSwingingToolLeft =true;
                    }
                    else if(playerDirection == Vector3Int.up)
                    {
                        isSwingingToolUp =true;
                    }
                    else if(playerDirection == Vector3Int.down)
                    {
                        isSwingingToolDown =true;
                    }
                    break;
            }

            Vector2 point = new Vector2(GetPlayerCentrePosition().x + (playerDirection.x * (equippedItemDetails.itemUseRadius / 2f )), GetPlayerCentrePosition().y +
                playerDirection.y * (equippedItemDetails.itemUseRadius / 2f));

            Vector2 size = new Vector2(equippedItemDetails.itemUseRadius, equippedItemDetails.itemUseRadius);

            Item[] itemArray = HelperMethods.GetComponentsAtBoxLocationNonAlloc<Item>(Settings.maxCollidersToTestPerReapSwing,point,size,0f);

            int reapableItemCount = 0;

            for(int i = itemArray.Length -1 ; i >= 0 ; i--)
            {

                if(itemArray[i] != null)
                {
                    if(InventoryManager.Instance.GetItemDetails(itemArray[i].ItemCode).itemType == ItemType.Reapable_scenary)
                    {

                        Vector3 effectPosition = new Vector3(itemArray[i].transform.position.x,itemArray[i].transform.position.y + Settings.gridCellSize / 2f,
                            itemArray[i].transform.position.z);


                            //reaping
                            EventHandler.CallHarvestActionEffectEvent(effectPosition,HarvestActionEffect.reaping);

                            Destroy(itemArray[i].gameObject);

                            reapableItemCount++;
                            if(reapableItemCount >= Settings.maxTargetComponentsToDestroyPerReapSwing)
                                break;
                    }
                }
            }
        }
    }



    private void ProcessCropWithEquippedItemInPlayerDirection(Vector3Int playerDirection,ItemDetails equippedItemDetails,GridPropertyDetails gridPropertyDetails)
    {
        switch(equippedItemDetails.itemType)
        {
            case ItemType.Collecting_tool:
                if(playerDirection == Vector3Int.right)
                {
                    isPickingRight =true;
                }
                else if(playerDirection == Vector3Int.left)
                {                        
                    isPickingLeft =true;
                }
                else if(playerDirection == Vector3Int.up)
                {
                    isPickingUp =true;
                }                    
                else if(playerDirection == Vector3Int.down)
                {
                    isPickingDown =true;
                }
                break;

            case ItemType.none:
                break;
                
        }

        Crop crop = GridPropertiesManager.Instance.GetCropObjectAtGridLocation(gridPropertyDetails);

        if(crop != null)
        {
            switch(equippedItemDetails.itemType)
            {
                case ItemType.Collecting_tool:
                    crop.ProcessToolAction(equippedItemDetails);
                    break;
            }
        }

    }










    private void PlayerTestInput()
    {
        if(Input.GetKey(KeyCode.T))
        {
            TimeManager.Instance.TestAdvanceGameMinute();
        }

        if(Input.GetKeyDown(KeyCode.G))
        {
            TimeManager.Instance.TestAdvanceGameDay();
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            SceneControllerManager.Instance.FadeAndLoadScene(SceneName.Scene1_Farm.ToString(),transform.position);
        }

        // if(Input.GetMouseButtonDown(1))
        // {
        //     GameObject tree = PoolManager.Instance.ReuseObject(canyonOakTreePrefab,mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
        //         Input.mousePosition.y, -mainCamera.transform.position.z)),Quaternion.identity);
        //         tree.SetActive(true);
        // }

    }




    private void ResetMovement()
    {
        xInput = 0f;
        yInput = 0f;
        isRunning = false;
        isWalking = false;
        isIdle = true;

    }



    public void DisablePlayerInputAndResetMovement()
    {
        DisablePlayerInput();
        ResetMovement();

        EventHandler.CallMovementEvent(xInput, yInput, isWalking, isRunning, isIdle, isCarrying,toolEffect,
            isUsingToolRight, isUsingToolLeft, isUsingToolUp, isUsingToolDown,
            isLiftingToolRight, isLiftingToolLeft, isLiftingToolUp, isLiftingToolDown,
            isPickingRight, isPickingLeft, isPickingUp, isPickingDown,
            isSwingingToolRight, isSwingingToolLeft, isSwingingToolUp, isSwingingToolDown,
            false, false, false, false);

    }

    public void DisablePlayerInput()
    {
        PlayerInputIsDisabled = true;
    }

    public void EnablePlayerInput()
    {
        PlayerInputIsDisabled = false;
    }

    public void ClearCarriedItem()
    {
        equippedItemSpriteRenderer.sprite = null;
        equippedItemSpriteRenderer.color = new Color(1f,1f,1f,0f);

        armsCharacterAttribute.partVariantType = PartVariantType.none;
        characterAttributeCustomisationList.Clear();
        characterAttributeCustomisationList.Add(armsCharacterAttribute);
        animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

        isCarrying = false;
    }


    public void ShowCarriedItem(int itemCode)
    {

        ItemDetails itemDetails = InventoryManager.Instance.GetItemDetails(itemCode);
        if(itemDetails != null)
        {
            equippedItemSpriteRenderer.sprite = itemDetails.itemSprite;
            equippedItemSpriteRenderer.color = new Color(1f,1f,1f,1f);

            armsCharacterAttribute.partVariantType = PartVariantType.carry;
            characterAttributeCustomisationList.Clear();
            characterAttributeCustomisationList.Add(armsCharacterAttribute);
            animationOverrides.ApplyCharacterCustomisationParameters(characterAttributeCustomisationList);

            isCarrying = true;

        }
    }



    public Vector3 GetPlayerViewportPosition()
    {
        return mainCamera.WorldToViewportPoint(transform.position);
    }


    public Vector3 GetPlayerCentrePosition()
    {
        return new Vector3(transform.position.x,transform.position.y + Settings.playerCentreYOffset,transform.position.z);
    }

}
