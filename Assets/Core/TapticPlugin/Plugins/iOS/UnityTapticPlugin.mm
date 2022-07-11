//
//  UnityTapticPlugin.m
//  unity-taptic-plugin
//
//  Created by Koki Ibukuro on 12/6/16.
//  Copyright Â© 2016 asus4. All rights reserved.
//

#import "UnityTapticPlugin.h"
#import <AudioToolbox/AudioServices.h>
#import <sys/utsname.h>

#pragma mark - UnityTapticPlugin

@interface UnityTapticPlugin ()
@property (nonatomic, strong) UINotificationFeedbackGenerator* notificationGenerator;
@property (nonatomic, strong) UISelectionFeedbackGenerator* selectionGenerator;
@property (nonatomic, strong) NSArray<UIImpactFeedbackGenerator*>* impactGenerators;
@end

@implementation UnityTapticPlugin

BOOL isTapticReady;
BOOL isIphone6s;
BOOL isIphone;

static UnityTapticPlugin * _shared;

+ (UnityTapticPlugin*) shared {
    @synchronized(self) {
        if(_shared == nil) {
            _shared = [[self alloc] init];
        }
    }
    return _shared;
}

- (id) init {
    
    if ([[[UIDevice currentDevice] systemVersion] compare:@"10.0" options:NSNumericSearch] == NSOrderedAscending)
    {
        isTapticReady = NO;
        return self;
    }
    
    if (self = [super init])
    {
        self.notificationGenerator = [UINotificationFeedbackGenerator new];
        [self.notificationGenerator prepare];
        
        self.selectionGenerator = [UISelectionFeedbackGenerator new];
        [self.selectionGenerator prepare];
        
        self.impactGenerators = @[
             [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight],
             [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium],
             [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy],
        ];
        for(UIImpactFeedbackGenerator* impact in self.impactGenerators) {
            [impact prepare];
        }
        
        struct utsname systemInfo;
        uname(&systemInfo);

	NSString* devName = [NSString stringWithCString:systemInfo.machine  encoding:NSUTF8StringEncoding];
        isIphone = ([devName rangeOfString:@"iPhone"].location != NSNotFound);
        if (isIphone) {
            NSArray* devNameNumbers = [[devName stringByReplacingOccurrencesOfString:@"iPhone" withString:@""] componentsSeparatedByString:@","];
            int devIndex = [[devNameNumbers objectAtIndex:0] intValue];
            isIphone6s = (devIndex == 8);
            isTapticReady = (devIndex >= 9);
        } else {
            isIphone6s = NO;
            isTapticReady = NO;
            return self;
        }
    }
    return self;
}

- (void) dealloc {
    self.notificationGenerator = NULL;
    self.selectionGenerator = NULL;
    self.impactGenerators = NULL;
}

- (void) notification:(UINotificationFeedbackType)type {
    
    if(isTapticReady){
        [self.notificationGenerator notificationOccurred:type];
    }else if(isIphone6s){
        AudioServicesPlaySystemSound(SystemSoundID(1521));
    }
}


- (void) selection {
    
    if(isTapticReady){
        [self.selectionGenerator selectionChanged];
    }else if(isIphone6s){
        AudioServicesPlaySystemSound(SystemSoundID(1520));
    }
}

- (void) impact:(UIImpactFeedbackStyle)style {
    
    if(isTapticReady){
        [self.impactGenerators[(int) style] impactOccurred];
    }else if(isIphone6s){
        if (style == UIImpactFeedbackStyleHeavy)
            AudioServicesPlaySystemSound(SystemSoundID(1520));
        else
            AudioServicesPlaySystemSound(SystemSoundID(1519));
    }
}

- (void) vibrate {
    
    if (isIphone){
        AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
    }
}

- (BOOL) isSupport {
    return (isTapticReady || isIphone6s);
}
    
- (BOOL) hasVibrator {
    return isIphone;
}

@end

#pragma mark - Unity Bridge

extern "C" {
    
    bool _unityTapticIsSupport() {
        return [[UnityTapticPlugin shared] isSupport];
    }
    
    void _unityTapticNotification(int type) {
        [[UnityTapticPlugin shared] notification:(UINotificationFeedbackType) type];
    }
    
    void _unityTapticSelection() {
        [[UnityTapticPlugin shared] selection];
    }
    
    void _unityTapticImpact(int style) {
        [[UnityTapticPlugin shared] impact:(UIImpactFeedbackStyle) style];
    }
    
    bool _unityHasVibrator() {
        return [[UnityTapticPlugin shared] hasVibrator];
    }

    void _unityVibrate() {
        [[UnityTapticPlugin shared] vibrate];
    }
    
}
