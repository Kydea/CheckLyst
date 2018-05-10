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
        
        if !Server.any(managedObjectContext: managedObjectContext) {
            
            if let server = NSEntityDescription.insertNewObject(forEntityName: "Server", into: managedObjectContext) as? Server {
                
                
                do {
                    
                    server.serverUrl = "https://checklystapi.jnj.com"
                    server.serverFrontEndUrl = "https://checklyst.jnj.com"
                    
                    try managedObjectContext.save()
                    
                }
                    
                catch {
                    
                }
                
            }
        }
        
    }
    
    static func setNewServerUrl(_ url: (api: String, frontEnd: String), managedObjectContext: NSManagedObjectContext) -> Bool {
        
        
        if let server = Server.getServer(managedObjectContext) {
            
            server.serverUrl = url.api
            server.serverFrontEndUrl = url.frontEnd
            
            do {
                
                try managedObjectContext.save()
                
                return true
                
            } catch {
                
                return false
                
            }
        }
        
        return false
        
    }
    
    static func getServer(_ managedObjectContext: NSManagedObjectContext) -> Server? {
        
        
        let request = NSFetchRequest<NSFetchRequestResult>(entityName: "Server")
        
        
        
        do {
            
            return (try managedObjectContext.fetch(request) as? [Server])?.first
            
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
