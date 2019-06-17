#import "BLE.h"
#import "BLEFramework.h"

NSString *const BLEUnityMessageName_OnBleDidInitialize = @"OnBleDidInitializeMessage";
NSString *const BLEUnityMessageName_OnBleDidConnect = @"OnBleDidConnectMessage";
NSString *const BLEUnityMessageName_OnBleDidCompletePeripheralScan = @"OnBleDidCompletePeripheralScanMessage";
NSString *const BLEUnityMessageName_OnBleDidDisconnect = @"OnBleDidDisconnectMessage";
NSString *const BLEUnityMessageName_OnBleDidReceiveData = @"OnBleDidReceiveDataMessage";

@interface BLEFrameworkDelegate() <BLEDelegate>
@property (strong,nonatomic) NSMutableArray *mDevices;
@property (strong, nonatomic) BLE *ble;
@property (nonatomic) BOOL isConnected;
@property (nonatomic) BOOL searchDevicesDidFinish;
@end

@implementation BLEFrameworkDelegate

- (id) init
{
    self = [super init];
    if (self)
    {
        self.ble = [[BLE alloc] init];
        
        if (self.ble)
        {
            [self.ble controlSetup];
            self.ble.delegate = self;
            self.mDevices = [[NSMutableArray alloc] init];
            [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidInitialize message:@"Success"];
        }
        else
        {
            [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidInitialize message:@"Fail"];
        }
    }
    else
    {
        [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidInitialize message:@"Fail"];
    }
    
    return self;
}



#pragma mark BLEDelegate methods
/*
 -(void) bleDidConnect;
 -(void) bleDidDisconnect;
 -(void) bleDidUpdateRSSI:(NSNumber *) rssi;
 -(void) bleDidReceiveData:(unsigned char *) data length:(int) length;
 -(void) bleDidChangeState: (CBManagerState *) state;
 */
-(void) bleDidConnect
{
    NSLog(@"->Connected");
    self.isConnected = true;
    
    /*
    // send reset
    UInt8 buf[] = {0xfe, 0xfe, 0xfe};
    NSData *data = [[NSData alloc] initWithBytes:buf length:3];
    [ble write:data];
    */
    
    [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidConnect message:@"Success"];
    
    // Schedule to read RSSI every 1 sec.
    //rssiTimer = [NSTimer scheduledTimerWithTimeInterval:(float)1.0 target:self selector:@selector(readRSSITimer:) userInfo:nil repeats:YES];
}

- (void)bleDidDisconnect
{
    self.isConnected = false;
    
    [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidDisconnect message:@"Success"];

    
    //[rssiTimer invalidate];
}

- (void)bleDidUpdateRSSI:(NSNumber *) rssi
{
    //self.rssiValue = rssi.stringValue;
}

- (void) bleDidReceiveData:(unsigned char *)data length:(int)length
{
    if (length > 0 && data != NULL) {
        NSLog(@"bleDidReceiveData length: %d", length);
        _dataRx = [NSData dataWithBytes:data length:length];
        NSLog(@"The data received is %x", ((unsigned char *)self.dataRx.bytes)[0]);
        [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidReceiveData message:[NSString stringWithFormat:@"%d",length]];
    } else {
        NSLog(@"bleDidReceiveData: empty data");
    }
}

-(void) bleDidChangeState: (CBManagerState) state {
    NSLog(@"state of ble: %ld", (long)state);
}
#pragma mark Instance Methodss
/*
 -(void) readRSSITimer:(NSTimer *)timer
 {
 [ble readRSSI];
 }
 */


- (void)scanForPeripherals
{
    self.searchDevicesDidFinish = false;
    
    if (self.ble.peripherals)
    {
        self.ble.peripherals = nil;
    }
    
    [self.ble findBLEPeripherals:2];
    
    [NSTimer scheduledTimerWithTimeInterval:(float)2.0 target:self selector:@selector(connectionTimer:) userInfo:nil repeats:NO];
}

-(void) connectionTimer:(NSTimer *)timer
{
    if (self.ble.peripherals.count > 0)
    {
        [self.mDevices removeAllObjects];
        
        int i;
        for (i = 0; i < self.ble.peripherals.count; i++)
        {
            CBPeripheral *p = [self.ble.peripherals objectAtIndex:i];
            
            if (p.identifier.UUIDString != NULL)
            {
                [self.mDevices insertObject:p.identifier.UUIDString atIndex:i];
            }
            else
            {
                [self.mDevices insertObject:@"UUID is NULL" atIndex:i];
            }
        }
        
        [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidCompletePeripheralScan message:@"Success"];

    }
    else
    {
        NSLog(@"No peripherals found");
        [self.mDevices removeAllObjects];
        [BLEFrameworkDelegate SendUnityMessage:BLEUnityMessageName_OnBleDidCompletePeripheralScan message:@"Fail: No device found"];
    }
    
    
    self.searchDevicesDidFinish = true;
}

- (bool)connectPeripheral:(NSString *)peripheralID
{
    CBPeripheral *selectedPeripheral;
    for (CBPeripheral *p in self.ble.peripherals)
    {
        if ([p.identifier.UUIDString isEqualToString:peripheralID])
        {
            selectedPeripheral = p;
        }
    }
    
    if (selectedPeripheral != nil)
    {
        [self.ble connectPeripheral:selectedPeripheral];
        return true;
    }
    
    return false;
}
- (bool)connectPeripheralAtIndex:(NSInteger)index
{
    if (index >= self.ble.peripherals.count)
    {
        return false;
    }
    else if ([self.ble.peripherals objectAtIndex:index]!=nil)
    {
        [self.ble connectPeripheral:[self.ble.peripherals objectAtIndex:index]];
        return true;
    }
    
    return false;
}

- (void)sendDataToPeripheral:(UInt8 *)buf length:(NSUInteger) length
{
    //UInt8 buf[3] = {0x01, 0x00, 0x00};
    NSData *data = [[NSData alloc] initWithBytes:buf length:length];
    [self.ble write:data];
}

#pragma mark Class Methods

+(void)SendUnityMessage:(NSString*)functionName arrayValuesToPass:(NSArray*)arrayValues
{
#ifdef UNITY_VERSION
    NSError *error;
    NSDictionary *jsonObjectToSerialize = [NSDictionary dictionaryWithObject:arrayValues forKey:@"data"];
    NSData *dictionaryData = [NSJSONSerialization dataWithJSONObject:jsonObjectToSerialize options:NSJSONWritingPrettyPrinted error:&error];
    NSString* jsonArrayValues = [NSString stringWithCString:(const char *)[dictionaryData bytes] encoding:NSUTF8StringEncoding];
    UnitySendMessage(strdup([@"BLEControllerEventHandler" UTF8String]), strdup([functionName UTF8String]), strdup([jsonArrayValues UTF8String]));
#endif
}

+(void)SendUnityMessage:(NSString*)functionName message:(NSString*)message
{   
#ifdef UNITY_VERSION
    UnitySendMessage(strdup([@"BLEControllerEventHandler" UTF8String]), strdup([functionName UTF8String]), strdup([message UTF8String]));   
#endif
}
@end


static BLEFrameworkDelegate* delegateObject = nil;

// Converts C style string to NSString
NSString* CreateNSString (const char* string)
{
    if (string)
        return [NSString stringWithUTF8String: string];
    else
        return [NSString stringWithUTF8String: ""];
}

// Helper method to create C string copy
char* MakeStringCopy (const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

// When native code plugin is implemented in .mm / .cpp file, then functions
// should be surrounded with extern "C" block to conform C function naming rules
extern "C" {
    
    void _InitBLEFramework ()
    {
        delegateObject = [[BLEFrameworkDelegate alloc] init];
    }
    
    void _ScanForPeripherals ()
    {
        [delegateObject scanForPeripherals];
    }
    
    bool _IsDeviceConnected()
    {
        return [delegateObject isConnected];
    }
    
    const char* _GetListOfDevices ()
    {
        if ([delegateObject searchDevicesDidFinish]/* && [delegateObject mDevices].count > 0*/)
        {
            NSError *error;
            NSDictionary *jsonObjectToSerialize = [NSDictionary dictionaryWithObject:[delegateObject mDevices] forKey:@"data"];
            NSData *dictionaryData = [NSJSONSerialization dataWithJSONObject:jsonObjectToSerialize options:NSJSONWritingPrettyPrinted error:&error];
            NSString* jsonArrayValues = [NSString stringWithCString:(const char *)[dictionaryData bytes] encoding:NSUTF8StringEncoding];
            
            return MakeStringCopy([jsonArrayValues UTF8String]);
        }
        
        return NULL;
    }
    
    bool _ConnectPeripheral(NSString * peripheralID)
    {
        return [delegateObject connectPeripheral:peripheralID];
    }
    
    bool _ConnectPeripheralAtIndex(int device)
    {
        return [delegateObject connectPeripheralAtIndex:(NSInteger)device];
    }
    
    void _SendData (unsigned char *buffer, int length)
    {
        [delegateObject sendDataToPeripheral:(UInt8 *)buffer length: length];
    }
    
    int _GetData(unsigned char **data, int size)
    {
        if (delegateObject.dataRx != nil) {
            memcpy(data, [delegateObject.dataRx bytes], size*sizeof(char));
            NSLog(@"The data saved is %x", *data);
            return 0;
        } else {
            NSLog(@"something is wrong. dataRx is nil");
            return -1;
        }
    }
}



