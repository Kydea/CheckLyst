//
//  FileManager+Utility.swift
//  Checklyst
//
//  Created by Matematici Developer on 28/08/2020.
//  Copyright © 2020 Johnson&Johnson. All rights reserved.
//

import Foundation

extension FileManager {
    
    func getAttribute(_ attributeKey : FileAttributeKey, fromFile: String) -> Any? {
        do {
            let attributes = try FileManager.default.attributesOfItem(atPath: fromFile) as [FileAttributeKey:Any]
            return attributes[attributeKey]
        } catch {
            return nil
        }
    }
    
    func getFileCreationDate(atPath path : String) -> Date? {
        return getAttribute(FileAttributeKey.creationDate, fromFile: path) as? Date
    }
    
    func getFileModificationDate(atPath path : String) -> Date? {
        return getAttribute(FileAttributeKey.modificationDate, fromFile: path) as? Date
    }
    
    func getDocumentsDirectory() -> URL {
        let paths = FileManager.default.urls(for: .documentDirectory, in: .userDomainMask)
        return paths[0]
    }
}
