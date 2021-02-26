//
//  UserDefaults+Utility.swift
//  Checklyst
//
//  Created by Matematici Developer on 26/02/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

extension UserDefaults {
    
    func existKey(_ key: String) -> Bool {
        return UserDefaults.standard.object(forKey: key) != nil
    }
    
    func date(forKey key: String) -> Date? {
        return UserDefaults.standard.object(forKey: key) as? Date
    }

}
