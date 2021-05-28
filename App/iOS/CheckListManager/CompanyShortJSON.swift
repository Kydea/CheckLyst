//
//  CompanyShortJSON.swift
//  Checklyst
//
//  Created by Matematici Developer on 27/05/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

public struct CompanyShortJSON: Decodable {
    
    init(id: Int, name: String) {
        self.id = id
        self.name = name
    }
    
    public let id: Int
    
    public let name: String

}
