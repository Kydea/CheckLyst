//
//  AppDelegate.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 21/06/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import UIKit
import CoreData


//@UIApplicationMain
class AppDelegate: UIResponder, UIApplicationDelegate {

    var window: UIWindow?
  
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplicationLaunchOptionsKey: Any]?) -> Bool {
        
        
        let navigationBarAppearace = UINavigationBar.appearance()
        
        UIApplication.shared.statusBarStyle = .lightContent
        
        if #available(iOS 13.0, *) {
            
            let navBarAppearance = UINavigationBarAppearance()
            let barButtonAppareance = UIBarButtonItemAppearance()
            
            barButtonAppareance.normal.titleTextAttributes = [NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 20)!,NSAttributedStringKey.foregroundColor: UIColor(red: 153.0/255, green: 153.0/255, blue: 153.0/255, alpha: 1.0)]
            barButtonAppareance.disabled.titleTextAttributes = [NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 20)!,NSAttributedStringKey.foregroundColor: UIColor.clear]
            
            navBarAppearance.buttonAppearance = barButtonAppareance;
            
            navBarAppearance.backgroundColor = UIColor(red: 64.0/255, green: 64.0/255, blue: 64.0/255, alpha: 1.0)
            
            navigationBarAppearace.standardAppearance = navBarAppearance
            navigationBarAppearace.compactAppearance = navBarAppearance
            navigationBarAppearace.scrollEdgeAppearance = navBarAppearance
        
        } else {
           
            UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 20)!,NSAttributedStringKey.foregroundColor: UIColor(red: 153.0/255, green: 153.0/255, blue: 153.0/255, alpha: 1.0)], for:UIControlState())

            UIBarButtonItem.appearance().setTitleTextAttributes([NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 20)!,NSAttributedStringKey.foregroundColor: UIColor.clear], for:.disabled)
            
            navigationBarAppearace.barTintColor = UIColor(red: 43.0/255, green: 43.0/255, blue: 43.0/255, alpha: 1.0)
        }
        
        navigationBarAppearace.tintColor = UIColor.white
        
        UITabBar.appearance().barTintColor = UIColor(red: 0.0/255, green: 150.0/255, blue: 136.0/255, alpha: 1.0)
        
        UITabBar.appearance().tintColor = UIColor.white
        
        Server.setupWithManagedObjectContext(managedObjectContext)
        

        UITabBarItem.appearance().setTitleTextAttributes([NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 15)!,NSAttributedStringKey.foregroundColor: UIColor.white], for:.selected)
        
        UITabBarItem.appearance().setTitleTextAttributes([NSAttributedStringKey.font:UIFont (name: "Flaticon", size: 15)!,NSAttributedStringKey.foregroundColor: UIColor(red: 3.0/255, green: 195.0/255, blue: 177.0/255, alpha: 1.0)], for:UIControlState())

        
        URLProtocol.registerClass(CustomUrlprotocol.self)
        
        return true
    }

    func applicationWillResignActive(_ application: UIApplication) {
        /*must be here to catch all pauses*/
        (UIApplication.shared as! MyApplication).goBackGround()
        // Sent when the application is about to move from active to inactive state. This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) or when the user quits the application and it begins the transition to the background state.
        // Use this method to pause ongoing tasks, disable timers, and throttle down OpenGL ES frame rates. Games should use this method to pause the game.
    }

    func applicationDidEnterBackground(_ application: UIApplication) {
        
        // Use this method to release shared resources, save user data, invalidate timers, and store enough application state information to restore your application to its current state in case it is terminated later.
        // If your application supports background execution, this method is called instead of applicationWillTerminate: when the user quits.
    }

    func applicationWillEnterForeground(_ application: UIApplication) {
        
        // Called as part of the transition from the background to the inactive state; here you can undo many of the changes made on entering the background.
    }

    func applicationDidBecomeActive(_ application: UIApplication) {
        
        (UIApplication.shared as! MyApplication).backFromBackGround()
        
        // Restart any tasks that were paused (or not yet started) while the application was inactive. If the application was previously in the background, optionally refresh the user interface.
    }

    func applicationWillTerminate(_ application: UIApplication) {
        // Called when the application is about to terminate. Save data if appropriate. See also applicationDidEnterBackground:.
        
    }
    
    
    lazy var applicationDocumentsDirectory: URL = {
        // The directory the application uses to store the Core Data store file. This code uses a directory named "Johnson-Johnson.prova" in the application's documents Application Support directory.
        let urls = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)
        return urls[urls.count-1]
    }()
    
    lazy var managedObjectModel: NSManagedObjectModel = {
        // The managed object model for the application. This property is not optional. It is a fatal error for the application not to be able to find and load its model.
        let modelURL = Bundle.main.url(forResource: "serverStorage", withExtension: "momd")!
        return NSManagedObjectModel(contentsOf: modelURL)!
    }()
    
    lazy var persistentStoreCoordinator: NSPersistentStoreCoordinator = {
        // The persistent store coordinator for the application. This implementation creates and returns a coordinator, having added the store for the application to it. This property is optional since there are legitimate error conditions that could cause the creation of the store to fail.
        // Create the coordinator and store
        let coordinator = NSPersistentStoreCoordinator(managedObjectModel: self.managedObjectModel)
        let url = self.applicationDocumentsDirectory.appendingPathComponent("CheckListManager.sqlite")
        var failureReason = "There was an error creating or loading the application's saved data."
        let options = [NSMigratePersistentStoresAutomaticallyOption: true, NSInferMappingModelAutomaticallyOption: true]
        do {
            try coordinator.addPersistentStore(ofType: NSSQLiteStoreType, configurationName: nil, at: url, options: options)
        } catch {
            // Report any error we got.
            var dict = [String: AnyObject]()
            dict[NSLocalizedDescriptionKey] = "Failed to initialize the application's saved data" as AnyObject
            dict[NSLocalizedFailureReasonErrorKey] = failureReason as AnyObject
            
            dict[NSUnderlyingErrorKey] = error as NSError
            let wrappedError = NSError(domain: "YOUR_ERROR_DOMAIN", code: 9999, userInfo: dict)
            // Replace this with code to handle the error appropriately.
            // abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development.
            NSLog("Unresolved error \(wrappedError), \(wrappedError.userInfo)")
            abort()
        }
        
        return coordinator
    }()
    
    lazy var managedObjectContext: NSManagedObjectContext = {
        // Returns the managed object context for the application (which is already bound to the persistent store coordinator for the application.) This property is optional since there are legitimate error conditions that could cause the creation of the context to fail.
        let coordinator = self.persistentStoreCoordinator
        var managedObjectContext = NSManagedObjectContext(concurrencyType: .mainQueueConcurrencyType)
        managedObjectContext.persistentStoreCoordinator = coordinator
        return managedObjectContext
    }()
    
    // MARK: - Core Data Saving support
    
    func saveContext () {
        if managedObjectContext.hasChanges {
            do {
                try managedObjectContext.save()
            } catch {
                // Replace this implementation with code to handle the error appropriately.
                // abort() causes the application to generate a crash log and terminate. You should not use this function in a shipping application, although it may be useful during development.
                let nserror = error as NSError
                NSLog("Unresolved error \(nserror), \(nserror.userInfo)")
                abort()
            }
        }
    }
    
    //MARK: - Application URL Responder
    
    func application(_ app: UIApplication, open url: URL, options: [UIApplicationOpenURLOptionsKey : Any] = [:]) -> Bool {
        let mApp = (app as! MyApplication)
        //print(url.path)
        if (url.getCallType == .Create ||
            url.getCallType == .Detail) {
            mApp.requestToOpenInstance = true
            mApp.requestedUrlInfo = url.getParametersForInstance
            if let logged = UserDefaults.standard.string(forKey: "logged") {
                if (NSString(string:logged).boolValue) {
                   mApp.openInstance()
                }
            } else {
                mApp.logoutAndGoToLogin()
            }
        }
        
        if (url.getCallType == .Setup) {
            let server = "https://" + url.host!
            mApp.setupNewServer(server, managedObjectContext: managedObjectContext)
        }
        
        return true
    }
    
   
}




