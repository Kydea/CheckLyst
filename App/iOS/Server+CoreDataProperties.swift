//
//  Server+CoreDataProperties.swift
//  CheckListManager
//
//  Created by Simone Testa on 10/05/2018.
//  Copyright © 2018 Johnson&Johnson. All rights reserved.
//
//

import Foundation
import CoreData


extension Server {

    @NSManaged public var serverUrl: String?
    @NSManaged public var serverFrontEndUrl: String?

}
