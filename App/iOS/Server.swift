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
    
    static func setupWithManagedObjectContext(_ managedObjectContext : NSManagedObjectContext) -> Void {
        
        if Server.any(managedObjectContext: managedObjectContext) {
            
            let request = NSFetchRequest<NSFetchRequestResult>(entityName: "Server")
            do {
                
                let server = (try managedObjectContext.fetch(request) as? [Server])!.first!
                setNewServer(urlFrontEnd: server.serverFrontEndUrl!, urlApi: server.serverUrl!, managedObjectContext: managedObjectContext)
                
            } catch {
                
            }
        }
        
    }
    
    static func setNewServer(urlFrontEnd:String, urlApi:String, managedObjectContext: NSManagedObjectContext) -> Bool {
        
        
        if let server = Server.getServerFromUrlFrontEnd(urlFrontEnd, managedObjectContext) {
            
            UserDefaults.standard.setValue(server.serverFrontEndUrl, forKey: "serverFrontEndUrl")
            UserDefaults.standard.setValue(server.serverUrl, forKey: "serverUrl")
            
            return true;
        }
        
        return false
        
    }
    
    static func addServerUrl(urlFrontEnd:String, urlApi:String, managedObjectContext: NSManagedObjectContext) -> Bool {
        
        if (!Server.any(NSPredicate(format: "serverFrontEndUrl = %@", urlFrontEnd), managedObjectContext: managedObjectContext)) {
        
            let server = Server(entity: Server.entity(), insertInto: managedObjectContext)
            server.serverFrontEndUrl = urlFrontEnd
            server.serverUrl = urlApi
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
    
    static func getServer(_ managedObjectContext: NSManagedObjectContext) -> Server? {
        
        if let serverFrontEndUrl = UserDefaults.standard.string(forKey: "serverFrontEndUrl") {
                return Server.getServerFromUrlFrontEnd(serverFrontEndUrl, managedObjectContext)
        }
        return nil
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
