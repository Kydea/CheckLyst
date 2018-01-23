//
//  ServerResponse.swift
//
//
//  Created by Lorenzo Spataro on 01/02/17.
//  Copyright © 2017 Matematici. All rights reserved.
//

import Foundation
import UIKit

open class ServerResponse: NSObject {

    var response: URLResponse?
    
    var error: NSError?
    
    var data: Data?
}
