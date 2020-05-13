//
//  Server.swift
//  CheckListManager
//
//  Created by Lorenzo Spataro on 14/07/16.
//  Copyright © 2016 Johnson&Johnson. All rights reserved.
//

import Foundation
import CoreData


class Server: NSManagedObject {
    
    // Insert code here to add functionality to your managed object subclass
    
    static func setNewServer(urlFrontEnd:String, apiUrl:String, managedObjectContext: NSManagedObjectContext) -> Bool {
        
        if Server.getServerFromUrlFrontEnd(urlFrontEnd, managedObjectContext) != nil {
            
            UserDefaults.standard.setValue(urlFrontEnd, forKey: "serverFrontEndUrl")
            UserDefaults.standard.setValue(apiUrl, forKey: "serverUrl")
            
            return true;
        }
        
        return false
        
    }
    
    static func unsetCurrentServer(managedObjectContext: NSManagedObjectContext) -> Bool {
        
        if  (isSetServer(managedObjectContext: managedObjectContext))  {
            
            UserDefaults.standard.removeObject(forKey: "serverFrontEndUrl")
            UserDefaults.standard.removeObject(forKey: "serverUrl")
            
            return true;
        }
        
        return false
        
    }
    
    static func isSetServer(managedObjectContext: NSManagedObjectContext) -> Bool {
           
        return UserDefaults.standard.object(forKey: "serverFrontEndUrl") != nil && UserDefaults.standard.object(forKey: "serverUrl") != nil
           
    }
    
    static func getFrontEndServer(managedObjectContext: NSManagedObjectContext) -> String? {
           
        return UserDefaults.standard.string(forKey: "serverFrontEndUrl")
           
    }
    
    static func getApiServer(managedObjectContext: NSManagedObjectContext) -> String? {
           
        return UserDefaults.standard.string(forKey: "serverUrl")
           
    }
    
    static func addServerUrl(urlFrontEnd:String, managedObjectContext: NSManagedObjectContext) -> Bool {
        
        if (!Server.any(NSPredicate(format: "serverFrontEndUrl = %@", urlFrontEnd), managedObjectContext: managedObjectContext)) {
        
            let server = Server(entity: Server.entity(), insertInto: managedObjectContext)
            server.serverFrontEndUrl = urlFrontEnd
        }
        
        do {
            try managedObjectContext.save()
            
            return true
            
        } catch {
            
            return false
            
        }
    }
    
    static func getServerFromUrlFrontEnd(_ urlFrontEnd:String, _ managedObjectContext: NSManagedObjectContext) -> Server? {
        
        let predicate = NSPredicate(format: "serverFrontEndUrl = %@", urlFrontEnd)
        let request = NSFetchRequest<NSFetchRequestResult>(entityName: "Server")
        request.predicate = predicate

        do {
            
            return (try managedObjectContext.fetch(request) as? [Server])?.first
            
        } catch {
            
            return nil
        }
        
    }
    
    static func getServers(_ managedObjectContext: NSManagedObjectContext) -> [Server]? {
           
        let request = NSFetchRequest<NSFetchRequestResult>(entityName: "Server")
        
        do {
               
            return (try managedObjectContext.fetch(request) as? [Server])
               
        } catch {
               
            return nil
        
        }
    }
    
    static func any(_ predicate: NSPredicate? = nil, managedObjectContext : NSManagedObjectContext) -> Bool {
        
        
        let request = NSFetchRequest<NSFetchRequestResult>(entityName: "Server")
        
        request.predicate = predicate
        
        return try! managedObjectContext.count(for: request) > 0
        
    }
    
}
