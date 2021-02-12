//
//  TokenSyncViewController.swift
//  Checklyst
//
//  Created by Matematici Developer on 12/02/2021.
//  Copyright © 2021 Johnson&Johnson. All rights reserved.
//

import Foundation

protocol TokenSyncViewController {
    func syncToken(accessToken : String, refreshToken : String)
}
